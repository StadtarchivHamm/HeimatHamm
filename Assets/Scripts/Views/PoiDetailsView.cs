using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Samples.ScreenReader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class PoiDetailsView : BaseView
{
	#region Fields
	#region Serialize Fields
	[Header("Header")]
	[SerializeField] private RawImage _poiImage;
	[SerializeField] private Button _poiImageFullscreenButton;
	[SerializeField] private FullscreenImageViewer _fullscreenImageViewer;
	[SerializeField] private ContrastButton _contrastButton;
	[SerializeField] private TextMeshProUGUI _titleText;
	[SerializeField] private TextMeshProUGUI _addressText;
	[SerializeField] private Button _navigationButton;
	[Space]
	[Header("Content")]
	[SerializeField] private GameObject _contentRoot;
	[SerializeField] private TextMeshProUGUI _descriptionText;
	[SerializeField] private Button _miniGameButton;
	[SerializeField] private AudioPlayer _audioDescriptionAudioPlayer;
	[SerializeField] private ImageCarrousel _carrousel;
    [SerializeField] private SecretPoiPopin _secretPoiUnlockPopin;
    [Space]
    [Header("Video")]
    [SerializeField] private Button _videoButton;
    [SerializeField] private RawImage _videoThumbnail;
	[SerializeField] private VideoManager _videoManager;
	[Space]
	[Header("Nearest POIs")]
	[SerializeField] private NearbyPoiListItem _poiListPrefab;
	[SerializeField] private Transform _nearestPoisRoot;
	[Space]
	[Header("Back")]
	[SerializeField] private Button _buttonBack;
	[Space]
	[Header("Bottom")]
	[SerializeField] private GameObject _bottomRoot;
	[SerializeField] private Button _showOnMapButton;
	[SerializeField] private Button _arButton;
	[SerializeField] private GameObject _arText;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_variables
	protected Wezit.Poi m_poiData;
	private Wezit.Relation m_poiImageRelation;
	private bool m_isSecretPoi;
	private string m_minigameType;
	#endregion Private m_Variables
	#endregion Fields

	#region Methods
	#region Private
	protected override void AddListeners()
	{
		base.AddListeners();

		_showOnMapButton.onClick.AddListener(delegate { SetState(KioskState.MAP); });
		_navigationButton.onClick.AddListener(delegate { OnPoiNavigationClicked(m_poiData.children.Find(x => x.tags.Contains(Tags.POI_LOCATION))); });

        _miniGameButton.onClick.AddListener(OnMinigameButtonClicked);
		_videoButton.onClick.AddListener(OnVideoButtonClicked);
		_buttonBack.onClick.AddListener(OnButtonBackClicked);

		if (!PlayerManager.CurrentState.IsAudioDescription)
		{
			_poiImageFullscreenButton.onClick.AddListener(OnPoiImageClicked);
		}

		_arButton.gameObject.SetActive(PlayerManager.Player.IsARCompatible);
		_arText.SetActive(PlayerManager.Player.IsARCompatible);
		_arButton.onClick.AddListener(OnQRCodeButtonClicked);

		_secretPoiUnlockPopin.SecretPopinClicked.AddListener(OnSecretPoiClicked);

    }

	protected override void RemoveListeners()
	{
		base.RemoveListeners();

        _navigationButton.onClick.RemoveAllListeners();
        _showOnMapButton.onClick.RemoveAllListeners();
        _miniGameButton.onClick.RemoveAllListeners();
		_videoButton.onClick.RemoveAllListeners();
        _buttonBack.onClick.RemoveAllListeners();
        _poiImageFullscreenButton.onClick.RemoveAllListeners();
		
		_arButton.onClick.RemoveAllListeners();

        _secretPoiUnlockPopin.SecretPopinClicked.RemoveListener(OnSecretPoiClicked);
    }

	protected override void ResetViewContent()
	{
		base.ResetViewContent();
		m_poiData = null;

		if (_titleText) _titleText.text = "";
		if (_descriptionText) _descriptionText.text = "";

		for (int i = 1; i < _nearestPoisRoot.childCount; i++)
		{
			Destroy(_nearestPoisRoot.GetChild(i).gameObject);
        }

        _contentRoot.transform.position = Vector3.zero;
		_videoManager.gameObject.SetActive(false);
        _videoButton.gameObject.SetActive(false);
        _secretPoiUnlockPopin.TogglePopin(false);
    }

	protected override async void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

		PlayerManager.CurrentState.IsFromOnMapButton = false;

		m_poiData = PlayerManager.CurrentState.CurrentPoi;

		if (PlayerManager.CurrentState.CurrentStationLocationPoi == null)
		{
			PlayerManager.CurrentState.CurrentStationLocationPoi = m_poiData.children.Find(x => x.tags.Contains(Tags.POI_LOCATION));
        }

        MatomoAnalyticsManager.Instance.RecordPoiVisited(PlayerManager.CurrentState.CurrentTour.CleanedTitle, m_poiData.pid, m_poiData.CleanedTitle);

        PoiProgressionData poiProgressionData = PlayerManager.Player.GetCurrentPoiProgression();

		if (!poiProgressionData.HasBeenVisited)
		{
			poiProgressionData.HasBeenVisited = true;
			PlayerManager.Player.Save();
		}
		m_isSecretPoi = m_poiData.tags.Contains(Tags.SECRET_POI);

        // Init text + prepare for contrast button
        _titleText.text = m_poiData.CleanedTitle;
		_titleText.GetComponent<AccessibleText>().SetLabel(m_poiData.CleanedTitle);
		_titleText.GetComponent<AccessibleText>().value = m_poiData.CleanedTitle;

		_addressText.text = m_poiData.children.Find(x => x.tags.Contains(Tags.POI_LOCATION)).CleanedSubject;
		_addressText.GetComponent<AccessibleText>().SetLabel(_addressText.text);
		_addressText.GetComponent<AccessibleText>().value =_addressText.text;

		_descriptionText.text = m_poiData.CleanedDescription;
		_descriptionText.GetComponent<AccessibleText>().SetLabel(m_poiData.CleanedDescription);
		_descriptionText.GetComponent<AccessibleText>().value = m_poiData.CleanedDescription;

		_contrastButton.Inflate(_titleText.text, new string[] { _addressText.text, _descriptionText.text }, _interfaceRoot.transform);

		_bottomRoot.SetActive(!PlayerManager.CurrentState.IsAudioDescription);
        _showOnMapButton.gameObject.SetActive(!PlayerManager.CurrentState.IsAudioDescription);
        _navigationButton.gameObject.SetActive(PlayerManager.CurrentState.IsGPSOn && PlayerManager.CurrentState.IsUserInTheArea && !PlayerManager.CurrentState.IsAudioDescription);

		m_minigameType = m_poiData.children.Find(x => x.tags.Contains(Tags.MINIGAME))?.type;
        _miniGameButton.gameObject.SetActive(!PlayerManager.CurrentState.IsAudioDescription && !m_isSecretPoi);
		if (!PlayerManager.Player.IsARCompatible && (m_minigameType.Contains(MinigameTypes.AR) || m_minigameType.Contains(MinigameTypes.MUSIC)))
		{
			_miniGameButton.gameObject.SetActive(false);
        }

        _audioDescriptionAudioPlayer.gameObject.SetActive(PlayerManager.CurrentState.IsAudioDescription);

		_poiImageFullscreenButton.enabled = !PlayerManager.CurrentState.IsAudioDescription;

		_navigationButton.gameObject.SetActive(!PlayerManager.CurrentState.IsAudioDescription);

		if (PlayerManager.CurrentState.IsAudioDescription)
		{
			_audioDescriptionAudioPlayer.Inflate(m_poiData);
        }

        await m_poiData.AreRelationsSet();
        InitImageAndVideo();

		InitCarrousel();

		InitNearestPOIs();

        if (PlayerManager.Player.GetCurrentTourProgression().PercentOfCompletion >= 1 && !PlayerManager.Player.HasSeenSecretPoiPopin)
        {
            _secretPoiUnlockPopin.Init(this);
            PlayerManager.Player.HasSeenSecretPoiPopin = true;
            PlayerManager.Player.Save();
        }

        this.DelayRefreshHierarchy();
    }

	private async void InitImageAndVideo()
	{
		m_poiImageRelation = await ImageUtils.LoadImage(_poiImage, this, PlayerManager.CurrentState.CurrentStationLocationPoi);

        // Init video button
        _videoButton.gameObject.SetActive(PlayerManager.CurrentState.CurrentStationLocationPoi.VideoRelations?.Count > 0);

        if (PlayerManager.CurrentState.CurrentStationLocationPoi.VideoRelations == null || PlayerManager.CurrentState.CurrentStationLocationPoi.VideoRelations?.Count == 0)
		{
			Debug.Log("No video in POI " + PlayerManager.CurrentState.CurrentStationLocationPoi.CleanedTitle);
			return;
        }

		if (await ImageUtils.HasCover(PlayerManager.CurrentState.CurrentStationLocationPoi, Wezit.RelationName.PLAY_VIDEO))
		{
			ImageUtils.LoadCover(_videoThumbnail, this, PlayerManager.CurrentState.CurrentStationLocationPoi, Wezit.RelationName.PLAY_VIDEO);
		}
		else
		{
			ImageUtils.LoadRefImage(_videoThumbnail, this, PlayerManager.CurrentState.CurrentStationLocationPoi);
        }
        _videoManager.Inflate(PlayerManager.CurrentState.CurrentStationLocationPoi.VideoRelations[0].GetAssetSourceByTransformation(WezitSourceTransformation.default_base));
	}

	private void InitCarrousel()
    {
		bool hasCarrousel = false;

        Wezit.Initializer.SetPoiChildren(m_poiData);

        foreach (Wezit.Poi childPoi in m_poiData.children)
		{
			if (childPoi.tags.Contains(Tags.CARROUSEL))
			{
				hasCarrousel = true;
				_carrousel.Inflate(childPoi, this);
				break;
			}
		}

		_carrousel.gameObject.SetActive(hasCarrousel);
	}

	private void InitNearestPOIs()
	{
		if (m_isSecretPoi)
        {
            _nearestPoisRoot.gameObject.SetActive(false);
            return;
		}

		Wezit.PoiLocation poiLocation = PoiLocationStore.GetPoiLocationById(PlayerManager.CurrentState.CurrentStationLocationPoi.pid);

		if (poiLocation == null)
        {
            _nearestPoisRoot.gameObject.SetActive(false);
            Debug.LogWarning("Poi location is null, this should not happen");
			return;
		}

		List<(Wezit.Poi poi, float distance)> nearestPois = new List<(Wezit.Poi poi, float distance)>();
		Vector2 poiLongLat = new Vector2(poiLocation.lng, poiLocation.lat);

        foreach (TourPoiLongLat tourChildPoi in PlayerManager.CurrentState.CurrentTourPoisLongLat)
        {
			if (PlayerManager.Player.GetPoiProgression(tourChildPoi.Poi.pid).HasBeenVisited)
			{
				continue;
			}

			if (tourChildPoi.Poi.pid == m_poiData.pid)
			{
				continue;
			}

			float distance = MapUtils.CalculateDistance(tourChildPoi.LongLat, poiLongLat);
			nearestPois.Add(new(tourChildPoi.Poi, distance));
        }

        _nearestPoisRoot.gameObject.SetActive(nearestPois.Count > 0);
        if (nearestPois.Count == 0)
		{
			Debug.LogWarning("There are no nearest POI, this is weird");
			return;
		}

		nearestPois.OrderBy(x => x.distance);

		for (int i = 0; i < Mathf.Min(nearestPois.Count, 3); i++)
		{
			NearbyPoiListItem nearbyPoi = Instantiate(_poiListPrefab, _nearestPoisRoot);
			nearbyPoi.Inflate(nearestPois[i].poi, PlayerManager.Player.GetPoiProgression(nearestPois[i].poi.pid), this);
            nearbyPoi.NearbyPoiClicked.AddListener(OnNearbyPoiClicked);
			nearbyPoi.NavigationButtonClicked.AddListener(OnPoiNavigationClicked);
		}
    }

    #region Buttons events
	private void OnPoiImageClicked()
	{
		_fullscreenImageViewer.Inflate(m_poiImageRelation);
	}

    private void OnMinigameButtonClicked()
	{
		switch (m_minigameType)
		{
			case MinigameTypes.AR:
				SetState(KioskState.MINIGAME_AR);
				break;
			case MinigameTypes.MUSIC:
				SetState(KioskState.MINIGAME_MUSIC);
				break;
			case MinigameTypes.DRAGDROP:
				SetState(KioskState.MINIGAME_DRAGDROP);
				break;
			case MinigameTypes.SLIDING_PUZZLE:
				SetState(KioskState.MINIGAME_SLIDING_PUZZLE);
				break;
			case MinigameTypes.TOUCH:
				SetState(KioskState.MINIGAME_TOUCH);
				break;
			case MinigameTypes.QUIZ:
				SetState(KioskState.MINIGAME_QUIZ);
				break;
			case MinigameTypes.DIAPORAMA:
				SetState(KioskState.MINIGAME_DIAPORAMA);
				break;
			default:
				break;
		}
	}

	private void OnVideoButtonClicked()
	{
		_videoManager.gameObject.SetActive(true);
		MenuManager.Instance.SetMenuStatus(MenuManager.MenuStatus.Hidden);
		Screen.orientation = ScreenOrientation.LandscapeLeft;
	}

	private void OnButtonBackClicked()
	{
		SetState(KioskState.LIST);
    }

	private void OnNearbyPoiClicked(Wezit.Poi poi)
	{

        PlayerManager.CurrentState.CurrentPoi = poi;
        AppManager.Instance.GoToState(KioskState.MAP);
    }

    private void OnPoiNavigationClicked(Wezit.Poi poi)
    {
        Wezit.PoiLocation poiLocation = PoiLocationStore.GetPoiLocationById(poi.pid);

        if (poiLocation == null)
        {
            return;
        }

        PlayerManager.CurrentState.NavigationGoalPOIPosition = new Vector2(poiLocation.lng, poiLocation.lat);
        PlayerManager.CurrentState.NavigationIsOn = true;
        AppManager.Instance.GoToState(KioskState.MAP);
    }

    private void OnQRCodeButtonClicked()
    {
		if (m_isSecretPoi)
		{
			SetState(KioskState.SECRET_POI);
			return;
		}

        SetState(KioskState.AR);
    }

	private void OnSecretPoiClicked()
    {
        MenuManager.Instance.KioskStateHistory.Pop();
        InitViewContentByLang(StoreAccessor.State.Language);
	}
    #endregion
    #endregion Private
    #endregion Methods
}