using System.Collections;
using TMPro;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Utils;
using Wezit;

public class ARView : BaseView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private Button _closeButton;
	[SerializeField] private Popin _closeWarningPopin;
	[Header("Intro")]
	[SerializeField] private GameObject _introRoot;
	[SerializeField] private AudioPlayer _introAudioPlayer;
	[Header("Hidden object")]
	[SerializeField] private GameObject _hiddenObjectRoot;
	[SerializeField] private Popin _hiddenObjectPopin;
	[SerializeField] private GameObject _objectFoundPanel;
	[SerializeField] private TextMeshProUGUI _objectFoundPanelTitle;
	[SerializeField]private RawImage _objectFoundThumbnail;
	[SerializeField] private Button _objectFoundPanelButton;
	[Header("Portal")]
	[SerializeField] private GameObject _portalRoot;
	[Header("Past")]
	[SerializeField] private GameObject _pastRoot;
	[SerializeField] private AudioPlayer _pastAudioPlayer;
	[SerializeField] private GameObject _allDonePanel;
	[SerializeField] private CanvasGroup _allDonePanelCanvasGroup;
	[SerializeField] private Button _allDonePanelTourButton;
	[SerializeField] private Button _allDonePanelARButton;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
	private ARManager m_arManager;
	private Poi m_poi;
	private bool m_aRIsLoaded;

    private Poi m_stationPositionPoi;
    private Poi m_avatarPositionPoi;
    private Poi m_hiddenObjectPoi;
    private Poi m_portalPositionPoi;
    private Poi m_pastObjectsPoi;

    private string m_avatarAudioStartDelaySettingKey = "ar.avatar.audio.start.delay.value";
    private string m_objectFoundTitleSettingKey = "ar.hidden.object.success.panel.title.text";
    private string m_pastAudioStartDelaySettingKey = "ar.avatar.audio.start.delay.value";
    #endregion Private m_Variables
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    public override void HideView()
	{
		if(m_aRIsLoaded)
        {
            PlayerManager.CurrentState.RuntimeReferenceImageLibrary = null;
            SceneManager.UnloadScene(1);

            m_aRIsLoaded = false;
        }

		ResetViewContent();
		base.HideView();
	}
	#endregion Public

	#region Private
	protected override void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

		m_poi = PlayerManager.CurrentState.CurrentPoi;

        m_stationPositionPoi = m_poi.children.Find(child => child.tags.Contains(Tags.POI_LOCATION));
        if (m_stationPositionPoi == null)
        {
            Debug.LogError("No station position POI");
        }

        m_avatarPositionPoi = m_poi.children.Find(child => child.tags.Contains(Tags.AVATAR));
        if (m_avatarPositionPoi == null)
        {
            Debug.LogError("No avatar POI");
        }

        m_hiddenObjectPoi = m_poi.children.Find(child => child.tags.Contains(Tags.HIDDEN_OBJECT));
        if (m_hiddenObjectPoi == null)
        {
            Debug.LogError("No hidden object POI");
        }

        m_portalPositionPoi = m_poi.children.Find(child => child.tags.Contains(Tags.PORTAL));
        if (m_portalPositionPoi == null)
        {
            Debug.LogError("No portal POI");
        }

        m_pastObjectsPoi = m_poi.children.Find(child => child.tags.Contains(Tags.PAST_OBJECTS));
        if (m_pastObjectsPoi == null)
        {
            Debug.LogError("No past object POI");
        }

		_closeWarningPopin.Inflate(false);


        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
		sceneLoading.completed += OnArLoaded;
	}

    protected override void ResetViewContent()
	{
		base.ResetViewContent();

		_introRoot.SetActive(false);

		_hiddenObjectRoot.SetActive(false);
		_hiddenObjectPopin.Close(false);
        _objectFoundPanel.SetActive(false);

        _portalRoot.SetActive(false);

        _pastRoot.SetActive(false);
        _allDonePanel.SetActive(false);
		_allDonePanelCanvasGroup.alpha = 0;

        _closeWarningPopin.Close(false);
    }

    protected override void AddListeners()
	{
		base.AddListeners();

		_closeButton.onClick.AddListener(OnCloseButton);
		_closeWarningPopin.PopinButtonClicked.AddListener(OnWarningPopinConfirm);
		_closeWarningPopin.PopinSecondaryButtonClicked.AddListener(OnWarningPopinCancel);

		_introAudioPlayer.AudioFinished.AddListener(OnIntroAudioFinished);

		_objectFoundPanelButton.onClick.AddListener(OnObjectFoundPanelButtonClicked);

        _pastAudioPlayer.AudioFinished.AddListener(OnPastAudioFinished);

        _allDonePanelTourButton.onClick.AddListener(OnAllDoneButtonClicked);
        _allDonePanelARButton.onClick.AddListener(OnAllDoneARButtonClicked);

        _introAudioPlayer.SubtitleContainsTag.AddListener(OnSubtitleContainsAnimationTag);
        _pastAudioPlayer.SubtitleContainsTag.AddListener(OnSubtitleContainsAnimationTag);

        _introAudioPlayer.SubtitleEmptinessChanged.AddListener(OnIntroSubtitleEmptinessChanged);
        _pastAudioPlayer.SubtitleEmptinessChanged.AddListener(OnPastSubtitleEmptinessChanged);
    }

    protected override void RemoveListeners()
	{
		base.RemoveListeners();

		_closeButton.onClick.RemoveAllListeners();
		_closeWarningPopin.PopinButtonClicked.RemoveAllListeners();
		_closeWarningPopin.PopinSecondaryButtonClicked.RemoveAllListeners();

        _introAudioPlayer.AudioFinished.RemoveAllListeners();

        _objectFoundPanelButton.onClick.RemoveAllListeners();

        _pastAudioPlayer.AudioFinished.RemoveAllListeners();

        _allDonePanelTourButton.onClick.RemoveAllListeners();
        _allDonePanelARButton.onClick.RemoveAllListeners();

		_introAudioPlayer.SubtitleContainsTag.RemoveAllListeners();
		_pastAudioPlayer.SubtitleContainsTag.RemoveAllListeners();

		_introAudioPlayer.SubtitleEmptinessChanged.RemoveAllListeners();
        _pastAudioPlayer.SubtitleEmptinessChanged.RemoveAllListeners();
    }

	private async void OnArLoaded(AsyncOperation loadingAsyncOperation)
	{
		await UniTask.Yield();
		m_aRIsLoaded = true;
		m_arManager = FindFirstObjectByType<ARManager>();
		m_arManager.Inflate(m_poi, m_stationPositionPoi, m_avatarPositionPoi, m_hiddenObjectPoi, m_portalPositionPoi, m_pastObjectsPoi);

		m_arManager.MarkerDetected.RemoveAllListeners();
		m_arManager.MarkerDetected.AddListener(OnMarkerDetected);

		m_arManager.HiddenObjectFound.RemoveAllListeners();
		m_arManager.HiddenObjectFound.AddListener(OnHiddenObjectFound);

		m_arManager.PastEntered.RemoveAllListeners();
		m_arManager.PastEntered.AddListener(OnPastEntered);
    }

    #region Close
    private void OnCloseButton()
	{
		_closeWarningPopin.Open();
	}

	private void OnWarningPopinConfirm()
	{
		SetState(KioskState.MAP);
	}

	private void OnWarningPopinCancel()
	{
		_closeWarningPopin.Close();
	}
    #endregion
    
	private void OnMarkerDetected()
    {
        _introAudioPlayer.Inflate(m_poi, startDelay:Settings.GetSettingAsFloat(m_avatarAudioStartDelaySettingKey, 10));
        _introRoot.SetActive(true);
    }

	private void OnIntroAudioFinished()
	{
		_introRoot.SetActive(false);
		_hiddenObjectRoot.SetActive(true);
		_hiddenObjectPopin.Inflate(true, setButtonToClose:true);
		m_arManager.OnIntroFinished();

        m_arManager.ToggleAvatarTalk(false);
    }

	private void OnHiddenObjectFound()
	{
		_objectFoundPanelTitle.text = string.Format(Settings.GetSettingAsCleanedText(m_objectFoundTitleSettingKey), m_hiddenObjectPoi.CleanedTitle); 
		ImageUtils.LoadImage(_objectFoundThumbnail, this, m_hiddenObjectPoi, fillParent:false);

		PlayerManager.Player.GetCurrentPoiProgression().HasCollectedItem = true;
		PlayerManager.Player.AddUnlockedHiddenObject(m_hiddenObjectPoi.pid);

		_objectFoundPanel.SetActive(true);
	}

	private void OnObjectFoundPanelButtonClicked()
	{
		_hiddenObjectRoot.SetActive(false);
		_portalRoot.SetActive(true);
		m_arManager.ShowPortal();
	}

	private void OnPastEntered()
	{
		_portalRoot.SetActive(false);
		_pastRoot.SetActive(true);

        _pastAudioPlayer.Inflate(m_pastObjectsPoi, startDelay: Settings.GetSettingAsFloat(m_pastAudioStartDelaySettingKey, 10));

        SetSeedCollected();
    }

	private void OnPastAudioFinished()
	{
		_allDonePanel.SetActive(true);
		m_arManager.PauseVideo(true);
	}

	private void OnAllDoneButtonClicked()
    {
        SetSeedCollected();
        MenuManager.Instance.KioskStateHistory.Pop();
        MenuManager.Instance.KioskStateHistory.Pop();
        SetState(KioskState.POI_DETAILS);
	}

	private void OnAllDoneARButtonClicked()
	{
		SetSeedCollected();
		_pastRoot.SetActive(false);
	}

	private void SetSeedCollected()
    {
        if (!PlayerManager.Player.GetCurrentPoiProgression().HasCollectedSeed)
        {
            PlayerManager.Player.GetCurrentPoiProgression().HasCollectedSeed = true;
			PlayerManager.Player.GetCurrentTourProgression().PercentOfCompletion += 1.0f / (PlayerManager.Player.GetCurrentTourProgression().PoisProgression.Count - 1);
            MenuManager.Instance.UpdateProgress();
            PlayerManager.Player.Save();
        }
    }

	private void OnSubtitleContainsAnimationTag(string animationTag)
	{
		m_arManager.StartAvatarAnimation(animationTag);
	}

	private void OnIntroSubtitleEmptinessChanged(bool isEmpty)
	{
		m_arManager.ToggleAvatarTalk(!isEmpty);
	}

	private void OnPastSubtitleEmptinessChanged(bool isEmpty)
	{
		m_arManager.PauseVideo(isEmpty);
	}
    #endregion Private
    #endregion Methods
}