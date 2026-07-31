using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Utils;

[Serializable]
public class TourEntryAndTag
{
	public HomeEntry TourEntry;
	public string Tag;
}

public class HomeView : BaseView
{
	#region Fields
	public static string TAG = "<color=orange>[HomeView]</color>";

	#region Serialize Fields
	[SerializeField] private List<TourEntryAndTag> _tourButtons;
	[SerializeField] private Transform _tourEntryRoot;
	[SerializeField] private SafeArea _safeArea;
    [Header("AR compatibility")]
    [SerializeField] private GameObject ARSessionPrefab;
	[SerializeField] private Popin _arCompatiblityPopin;

    #endregion Serialize Fields

    #region Public Variables
    #endregion Public Variables

    #region Private m_Variables
    private List<Wezit.Tour> m_tours = new List<Wezit.Tour>();

	private string m_splashBackSettingKey = "template.mobile2.language.screen.background.back.image";
	private string m_splashFrontSettingKey = "template.mobile2.language.screen.background.front.image";
	private string m_splashVideoSettingKey = "splash.screen.background.video";

    private GameObject ARSessionObj;
    #endregion Private m_Variables
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    #endregion Public

    #region Private
    protected override async void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

        BGMusicManager.Instance.FadeSoundOut(1);

        // Stop location service just in case to save some computing power and battery
        MapUtils.StopLocationService();

        MenuManager.Instance.SetMenuStatus(MenuManager.MenuStatus.Home);

		m_tours = WezitDataUtils.GetWezitToursByLang(language);

		foreach (TourEntryAndTag tourEntryAndTag in _tourButtons)
		{
			Wezit.Tour tour = m_tours.Find(x => x.tags.ToLower().Contains(tourEntryAndTag.Tag.ToLower()));
            tourEntryAndTag.TourEntry.Inflate(m_tours.Find(x => x.tags.Contains(tourEntryAndTag.Tag)), tourEntryAndTag.Tag == "audiodescription", tourEntryAndTag.Tag == "easytoread");
            tourEntryAndTag.TourEntry.HomeEntryClicked.RemoveListener(OnTourButtonClicked);
            tourEntryAndTag.TourEntry.HomeEntryClicked.AddListener(OnTourButtonClicked);
		}

		_tourButtons.Find(x => x.Tag == "easytoread").TourEntry.gameObject.SetActive(language == Language.de);

		if (!PlayerManager.Player.CheckedARCompatibility)
        {
            PlayerManager.Player.CheckedARCompatibility = true;

            PlayerManager.Player.IsARCompatible = await CheckCompatibility.IsCompatible(this);
            Debug.Log(TAG + " IsARCompatible: " + PlayerManager.Player.IsARCompatible);

            if (!PlayerManager.Player.IsARCompatible && !PlayerManager.Player.HasSeenCompatibilityPopin)
            {
                Debug.Log(TAG + " Opening compatibility popin");
                _arCompatiblityPopin.Inflate(true, setButtonToClose:true);
                PlayerManager.Player.HasSeenCompatibilityPopin = true;
            }
            PlayerManager.Player.Save();
        }

		DownloadSplashImages();
	}

	protected override void ResetViewContent()
	{
		base.ResetViewContent();
		m_tours.Clear();

        if (PlayerManager.CurrentState != null)
        {
			PlayerManager.CurrentState.CurrentTour = null;
			PlayerManager.CurrentState.CurrentPoi = null;
        }
	}

	protected override void AddListeners()
	{
		base.AddListeners();
	}

	protected override void RemoveListeners()
	{
		base.RemoveListeners();
	}

	private void OnTourButtonClicked(Wezit.Tour tour, bool isAudioDescription, bool isEasyLanguage)
    {
		PlayerManager.CurrentState.CurrentTour = tour;

		PlayerManager.CurrentState.CurrentAvatarType = tour.tags.Contains("toni") ? AvatarManager.AvatarType.Toni :
													   tour.tags.Contains("easytoread") ? AvatarManager.AvatarType.Toni :
													   tour.tags.Contains("grete") ? AvatarManager.AvatarType.Grete :
													   AvatarManager.AvatarType.Klippi;
		PlayerManager.CurrentState.IsAudioDescription = isAudioDescription;
		PlayerManager.CurrentState.IsEasyToRead = isEasyLanguage;

        AppManager.Instance.GoToState(KioskState.TOUR_INTRO);
    }

	private void DownloadSplashImages()
	{
		Wezit.Settings.DownloadSettingMedia(m_splashBackSettingKey, System.IO.Path.Combine(Application.persistentDataPath, "splash_back.jpg"));
		Wezit.Settings.DownloadSettingMedia(m_splashFrontSettingKey, System.IO.Path.Combine(Application.persistentDataPath, "splash_front.jpg"));

		Wezit.Settings.DownloadSettingMedia(m_splashVideoSettingKey, System.IO.Path.Combine(Application.persistentDataPath, "splash.mp4"));
    }
    #endregion Private

    #region Internals
    #endregion Internals
    #endregion Methods
}