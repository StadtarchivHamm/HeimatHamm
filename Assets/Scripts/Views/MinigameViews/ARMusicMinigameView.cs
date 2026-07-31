using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using Wezit;

public class ARMusicMinigameView : MinigameView
{
    #region Fields
    #region Serialize Fields
    [Space]
    [SerializeField] private AudioPlayer _resultPopinAudioPlayer;
    [SerializeField] private Transform _resultPopinNotesRoot;
    [SerializeField] private GameObject _resultPopinNotePrefab;
    #endregion Serialize Fields

    #region Public Variables
    #endregion Public Variables

    #region Private m_Variables
    #endregion Private m_Variables
    private ARMusicMinigameManager m_arMinigameManager;

    private bool m_aRIsLoaded;

    private int m_numberOfItems;
	private float m_spawnRate;
	private float m_lifetime;
	private float m_radius;
	private float m_scale;

    private int m_numberOfHitItems;

    private List<Poi> m_itemsPois = new List<Poi>();
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    #endregion Public
    public override void HideView()
    {
        if (m_aRIsLoaded)
        {
            SceneManager.UnloadSceneAsync(3);
            m_aRIsLoaded = false;
        }
        base.HideView();
    }

    #region Private
    protected override void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

		m_numberOfItems = (int)StringUtils.GetStringAsFloat(m_minigamePoi.extent);
        m_spawnRate = StringUtils.GetStringAsFloat(m_minigamePoi.location);
        m_lifetime = StringUtils.GetStringAsFloat(m_minigamePoi.author);
        m_radius = StringUtils.GetStringAsFloat(m_minigamePoi.source);
        m_scale = StringUtils.GetStringAsFloat(m_minigamePoi.spatial);

        m_itemsPois = m_minigamePoi.GetChildren();
        if (!m_aRIsLoaded)
        {
            AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);
            sceneLoading.completed += OnArLoaded;
        }
        else
        {
            OnArLoaded(null);
        }
    }

    protected override void ResetViewContent()
    {
        base.ResetViewContent();

        foreach (Transform child in _resultPopinNotesRoot)
        {
            Destroy(child.gameObject);
        }

        m_itemsPois.Clear();
        m_numberOfHitItems = 0;
    }

    private void OnArLoaded(AsyncOperation loadingAsyncOperation)
    {
        m_aRIsLoaded = true;
        m_arMinigameManager = FindFirstObjectByType<ARMusicMinigameManager>();

        m_arMinigameManager.Inflate(m_numberOfItems, m_spawnRate, m_lifetime, m_radius, m_scale, m_itemsPois);
        m_arMinigameManager.PlayerHitObject.RemoveAllListeners();
        m_arMinigameManager.PlayerHitObject.AddListener(OnPlayerHitObject);
    }

    protected override void OnInstructionPopinClosed()
    {
        if (!m_started)
        {
            m_arMinigameManager.StartGame();
        }

        base.OnInstructionPopinClosed();
    }

    private void OnPlayerHitObject(ARGameItem aRGameItem)
    {
        if (aRGameItem.IsPositive)
        {
            m_numberOfHitItems++;
            m_arMinigameManager.GoodObjectHit(aRGameItem);

            if (m_numberOfHitItems >= m_numberOfItems)
            {
                OnActivityOver(m_arMinigameManager.CollectedNotes);
                m_arMinigameManager.EndGame();
            }
        }
    }

    protected void OnActivityOver(List<ARMusicGameCollectedNote> collectedNotes)
    {
        bool playerCollectedOneTypeOfNote = true;
        string lastPid = collectedNotes[0].Poi.pid;

        for (int i = 0; i < collectedNotes.Count - 1; i++)
        {
            playerCollectedOneTypeOfNote = playerCollectedOneTypeOfNote && (collectedNotes[i].Poi.pid == lastPid);
            lastPid = collectedNotes[i].Poi.pid;
            Instantiate(_resultPopinNotePrefab, _resultPopinNotesRoot).GetComponentInChildren<RawImage>().texture = collectedNotes[i].CollectedNoteThumbnail;
        }

        // Last collected note did not have the time to load its texture before opening the popin, so we have to load it in parallel
        ImageUtils.LoadRefImage(Instantiate(_resultPopinNotePrefab, _resultPopinNotesRoot).GetComponentInChildren<RawImage>(), this, collectedNotes[collectedNotes.Count - 1].Poi, fillParent: false);

        OnActivityOver(playerCollectedOneTypeOfNote);
        _resultPopinAudioPlayer.Inflate(playerCollectedOneTypeOfNote ? collectedNotes[0].Poi : m_minigamePoi, true);
    }
    #endregion Private

    #region Internals
    #endregion Internals
    #endregion Methods
}