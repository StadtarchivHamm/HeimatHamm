using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;

public class AudioPlayer : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ImprovedToggle _playPauseToggle;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _totalTimeText;
    [Space]
    [SerializeField] private SubtitleDisplayerNoFade _subtitlesContainer;
    [SerializeField] private Toggle _subtitlesDisplayToggle;
    #endregion
    #region Private
    private AudioClip m_audioClip;
    private bool m_isPaused;
    private int m_minutes;
    private int m_seconds;
    private float m_subSeconds;
    private int m_inflateSessionId;
    #endregion
    #endregion

    #region Properties
    public UnityEvent AudioFinished = new UnityEvent();
    public UnityEvent<string> SubtitleContainsTag = new UnityEvent<string>();
    public UnityEvent<bool> SubtitleEmptinessChanged = new UnityEvent<bool>();
    #endregion

    #region Methods
    #region Monobehaviours
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    #endregion
    #region Public
    public async void Inflate(Wezit.Node node, bool isAmbiantSound = false, float startDelay = 0)
    {
        Inflate(await (isAmbiantSound ? AudioUtils.GetAmbiantAudioSource(node) : AudioUtils.GetAudioSource(node)), startDelay);

        if (_subtitlesContainer != null)
        {
            _subtitlesContainer.Inflate(node, startReading:false, lookForTags:true);
            _subtitlesContainer.SubtitleContainsTag.RemoveListener(OnSubtitleContainsTag);
            _subtitlesContainer.SubtitleContainsTag.AddListener(OnSubtitleContainsTag);

            _subtitlesContainer.SubtitleEmptinessChanged.RemoveListener(OnSubtitleEmptinessChanged);
            _subtitlesContainer.SubtitleEmptinessChanged.AddListener(OnSubtitleEmptinessChanged);

            _subtitlesDisplayToggle.onValueChanged.RemoveListener(_subtitlesContainer.HideSubtitles);
            _subtitlesDisplayToggle.onValueChanged.AddListener(_subtitlesContainer.HideSubtitles);
        }
    }

    public async void Inflate(string audioSourceUri, float startDelay = 0)
    {
        int sessionId = ++m_inflateSessionId;
        m_audioClip = await AudioUtils.GetAudioClip(audioSourceUri);

        if (sessionId != m_inflateSessionId) return;

        gameObject.SetActive(m_audioClip != null);
        if (m_audioClip == null)
        {
            return;
        }

        _audioSource.clip = m_audioClip;
        _audioSource.time = 0;

        _playPauseToggle.SetIsOnWithoutNotify(false);
        _playPauseToggle.onValueChanged.RemoveListener(OnAudioToggled);
        _playPauseToggle.onValueChanged.AddListener(OnAudioToggled);

        m_isPaused = true;

        if (_progressBar != null)
        {
            _progressBar.value = 0;
            _progressBar.minValue = 0;
            _progressBar.maxValue = m_audioClip.length;
            _progressBar.onValueChanged.RemoveListener(OnSliderValueChanged);
            _progressBar.onValueChanged.AddListener(OnSliderValueChanged);

            _progressText.text = "00:00";

            if (_totalTimeText != null)
            {
                int clipLength = Mathf.FloorToInt(m_audioClip.length);
                int minutes = clipLength / 60;
                int seconds = clipLength - minutes * 60;

                _totalTimeText.text = clipLength == 0 ? "--:--" : string.Format("{0}:{1}", minutes.ToString("00"), seconds.ToString("00"));
            }
        }

        m_subSeconds = m_seconds = m_minutes = 0;

        await Task.Delay(Mathf.FloorToInt(startDelay * 1000));

        if (sessionId != m_inflateSessionId || !m_isPaused)
        {
            return;
        }
        _playPauseToggle.isOn = true;
        if (_subtitlesContainer != null)
        {
            _subtitlesContainer.StartReading();
        }
    }
    #endregion
    #region Private
    private void OnAudioToggled(bool isPlay)
    {
        m_isPaused = isPlay;

        if (_subtitlesContainer != null) 
        { 
            _subtitlesContainer.TogglePause(!isPlay);
        }

        if (isPlay)
        {
            if (_audioSource.time == 0)
            {
                _audioSource.Play();
                StartCoroutine(ProgressCoroutine(_audioSource.clip.length));
                if (_subtitlesContainer != null)
                {
                    _subtitlesContainer.StartReading();
                }
            }
            else
            {
                _audioSource.UnPause();
            }
        }
        else
        {
            _audioSource.Pause();
        }
    }

    private IEnumerator ProgressCoroutine(float audioLength)
    {
        while (_audioSource.time < _audioSource.clip.length - 0.1f)
        {
            if (m_isPaused)
            {
                yield return null;
            }

            if (_progressBar != null)
            {
                _progressBar.SetValueWithoutNotify(_audioSource.time);
            }

            m_subSeconds = _audioSource.time - m_seconds - 60 * m_minutes;

            if (m_subSeconds >= 1)
            {
                m_seconds++;
                m_subSeconds = 0;

                if (_progressText != null)
                {
                    _progressText.text = string.Format("{0}:{1}", m_minutes.ToString("00"), m_seconds.ToString("00"));
                }
            }
            if (m_seconds >= 60)
            {
                m_minutes++;
                m_seconds = 0;
            }

            yield return null;
        }

        _playPauseToggle.isOn = false;
        _audioSource.time = 0;

        if (_progressText != null)
        {
            _progressBar.SetValueWithoutNotify(0);
            _progressText.text = "00:00";
        }

        AudioFinished?.Invoke();
    }

    private void OnSliderValueChanged(float value)
    {
        _audioSource.time = value;

        if (_progressText != null)
        {
            m_minutes = Mathf.FloorToInt(value / 60);
            m_seconds = Mathf.FloorToInt(value - m_minutes * 60);

            m_subSeconds = _audioSource.time - m_seconds - 60 * m_minutes;
            _progressText.text = string.Format("{0}:{1}", m_minutes.ToString("00"), m_seconds.ToString("00"));
        }

        if (_subtitlesContainer != null)
        {
            Debug.Log("Slider value changed");
            _subtitlesContainer.SetTime(value);
        }
    }

    private void OnSubtitleContainsTag(string tag)
    {
        SubtitleContainsTag?.Invoke(tag);
    }

    private void OnSubtitleEmptinessChanged(bool isEmpty)
    {
        SubtitleEmptinessChanged?.Invoke(isEmpty);
    }
    #endregion
    #endregion
}
