using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Utils;

public class SubtitleDisplayerNoFade : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    public TextAsset Subtitle;
    [SerializeField] private string SubtitlesString;
    [SerializeField] private TextMeshProUGUI _bgText;
    [SerializeField] private TextMeshProUGUI _currentlyDiplayingText;
    [SerializeField] private CanvasGroup _canvasGroup;
    #endregion

    #region Private
    private bool m_isPaused;
    public float m_elapsedTime;
    private double m_nextSubtitleFrom = 0;
    private SRTParser m_parser;

    private Coroutine m_readingCoroutine;

    private bool m_lookForTags;
    private bool m_previousSubtitleWasEmpty;
    private bool m_updateSubtitle = false;
    #endregion
    #endregion

    #region Properties
    public UnityEvent<string> SubtitleContainsTag = new UnityEvent<string>();
    public UnityEvent<bool> SubtitleEmptinessChanged = new UnityEvent<bool>();
    #endregion

    #region Methods
    #region Public
    public async void Inflate(Wezit.Node poi, bool startReading = true, bool lookForTags = false)
    {
        _bgText.gameObject.SetActive(false);
        await poi.AreRelationsSet();
        if (poi.SubtitlesRelations?.Count == 0)
        {
            return;
        }
        string subtitlesString = await FileUtils.RequestTextContent(poi.SubtitlesRelations[0].GetAssetSourceByTransformation(WezitSourceTransformation.original), 5);

        Inflate(subtitlesString, startReading, lookForTags);
    }

    public void Inflate(TextAsset subtitle, bool startReading = true, bool lookForTags = false)
    {
        m_lookForTags = lookForTags;

        Subtitle = subtitle;

        if (startReading)
        {
            StartReading();
        }
    }

    public void Inflate(string subtitlesString, bool startReading = true, bool lookForTags = false)
    {
        m_lookForTags = lookForTags;

        SubtitlesString = subtitlesString;

        if (startReading)
        {
            StartReading();
        }
    }

    public void HideSubtitles(bool hideSubtitles)
    {
        _canvasGroup.alpha = hideSubtitles ? 0 : 1;
    }

    public void TogglePause(bool pause)
    {
        m_isPaused = pause;
    }

    public void SetTime(float time)
    {
        m_elapsedTime = time;
        m_updateSubtitle = true;
    }

    public void StartReading()
    {
        if (m_readingCoroutine != null)
        {
            StopCoroutine(m_readingCoroutine);
        }
        m_readingCoroutine = StartCoroutine(ReadSubtitles());
    }


    public void StopReading()
    {
        if (m_readingCoroutine != null)
        {
            StopCoroutine(m_readingCoroutine);
        }
        _bgText.gameObject.SetActive(false);
    }
    #endregion

    #region Private
    private IEnumerator ReadSubtitles()
    {
        _currentlyDiplayingText.text = string.Empty;
        _bgText.text = string.Empty;
        _currentlyDiplayingText.gameObject.SetActive(true);
        _bgText.gameObject.SetActive(true);
        m_elapsedTime = 0;
        m_nextSubtitleFrom = 0;

        if (SubtitlesString != null)
        {
            m_parser = new SRTParser(SubtitlesString);
        }
        else if (Subtitle != null)
        {
            m_parser = new SRTParser(Subtitle);
        }
        else
        {
            Debug.LogError("No subtitle string or text asset!");
        }

        SubtitleBlock currentSubtitle = null;
        SubtitleBlock subtitle = null;
        double currentSubtitleTo = 0;

        while (true)
        {
            if (m_isPaused)
            {
                yield return null;
                continue;
            }

            m_elapsedTime += Time.deltaTime;

            if (m_elapsedTime >= m_nextSubtitleFrom || m_updateSubtitle)
            {
                subtitle = m_parser.GetForTime(m_elapsedTime);

                if (m_updateSubtitle)
                {
                    m_nextSubtitleFrom = 0;
                    currentSubtitleTo = 0;
                    m_updateSubtitle = false;
                }
            }

            if (m_elapsedTime >= currentSubtitleTo && m_elapsedTime < m_nextSubtitleFrom)
            {
                subtitle = SubtitleBlock.Blank;
            }

            if (subtitle != null)
            {
                if (!subtitle.Equals(currentSubtitle))
                {
                    currentSubtitle = subtitle;
                    if (currentSubtitle.Index > 0 && currentSubtitle.Index < m_parser.Subtitles.Count)
                    {
                        m_nextSubtitleFrom = m_parser.Subtitles[currentSubtitle.Index].From;
                    }
                    currentSubtitleTo = subtitle.To;
                    _bgText.gameObject.SetActive(!string.IsNullOrEmpty(currentSubtitle.Text));

                    if ((m_previousSubtitleWasEmpty && !string.IsNullOrEmpty(currentSubtitle.Text)) ||
                        (!m_previousSubtitleWasEmpty && string.IsNullOrEmpty(currentSubtitle.Text)))
                    {
                        SubtitleEmptinessChanged?.Invoke(string.IsNullOrEmpty(currentSubtitle.Text));
                        m_previousSubtitleWasEmpty = string.IsNullOrEmpty(currentSubtitle.Text);
                    }

                    string subtitleText = currentSubtitle.Text;
                    if (m_lookForTags)
                    {
                        if (subtitleText.Contains("["))
                        {
                            string subtitleTag = subtitleText.Split('[', ']')[1];
                            Debug.Log("Subtitle contains tag " + subtitleTag);
                            SubtitleContainsTag?.Invoke(subtitleTag);
                            subtitleText = subtitleText.Replace("[" + subtitleTag + "]", "");
                        }
                    }

                    // Switch subtitle text
                    _currentlyDiplayingText.text = subtitleText;
                    _bgText.text = subtitleText;

                }

                yield return null;
            }
            else
            {
                Debug.Log("Subtitles ended");
                _currentlyDiplayingText.gameObject.SetActive(false);
                _bgText.gameObject.SetActive(false);
                yield break;
            }
        }
    }
    #endregion
    #endregion
}
