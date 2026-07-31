using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using Unity.Samples.ScreenReader;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UI;
using Utils;

public class TourIntroView : BaseView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private RawImage _tourBackgroundImage;
	[SerializeField] private RawImage _tourCharacterImage;
	[SerializeField] private TextMeshProUGUI _title;
	[SerializeField] private TextMeshProUGUI _subtitle;
	[SerializeField] private AudioPlayer _audioDescription;
	[SerializeField] private TextMeshProUGUI _description;
	[Space]
	[SerializeField] private Button _startButton;
	[SerializeField] private GameObject _downloadButtonsRoot;
	[SerializeField] private Button _startWithoutDownloadButton;
	[SerializeField] private TextMeshProUGUI _downloadSizeText;
	[SerializeField] private Button _downloadButton;
	[Space]
	[SerializeField] private ContrastButton _contrastButton;
	[SerializeField] private Transform _contrastPanelRoot;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
	private Wezit.Tour m_TourData;
	
	private int m_downloadSize;
	#endregion Private m_Variables
	#endregion Fields

	#region Properties
	#endregion Properties

	#region Methods
	#region Public
	#endregion Public

	#region Private
	protected override void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

		m_TourData = PlayerManager.CurrentState.CurrentTour;

		if (m_TourData.RefPictureRelations?.Count > 1)
		{
			string tourCharacterSpriteSource = m_TourData.RefPictureRelations[1].GetAssetSourceByTransformation("default");
            StartCoroutine(TextureAndSpriteUtils.GetSpriteFromSource(tourCharacterSpriteSource, OnTourSpriteDownloaded));
		}

		ImageUtils.LoadImage(_tourBackgroundImage, this, m_TourData);
		ImageUtils.LoadRefImage(_tourCharacterImage, this, m_TourData);

		string[] splitTitle = m_TourData.CleanedTitle.Split('|');
        _title.text = splitTitle.Length > 1 ? splitTitle[1] : m_TourData.CleanedTitle;
		_title.GetComponentInChildren<AccessibleText>().SetLabel(m_TourData.CleanedTitle);
		_title.GetComponentInChildren<AccessibleText>().value = m_TourData.CleanedTitle;

		_subtitle.text = m_TourData.CleanedSubject;
		_subtitle.GetComponentInChildren<AccessibleText>().SetLabel(m_TourData.CleanedSubject);
		_subtitle.GetComponentInChildren<AccessibleText>().value = m_TourData.CleanedSubject;
        
		_description.text = m_TourData.CleanedDescription;
		_description.GetComponentInChildren<AccessibleText>().SetLabel(m_TourData.CleanedDescription);
		_description.GetComponentInChildren<AccessibleText>().value = m_TourData.CleanedDescription;
        string[] paragraphs = { _subtitle.text, _description.text };
        _contrastButton.Inflate(_title.text, paragraphs, _contrastPanelRoot);

		_audioDescription.Inflate(m_TourData);

        // Start location service so that the user has the distance to the POI as soon as they leave this screen
        MapUtils.StartLocationService(this);

        // Check download necessity
        if (PlayerManager.Player.GetTourProgression(m_TourData.pid).HasBeenDownloaded)
		{
			m_downloadSize = Wezit.DataGrabber.Instance.GetUpdateSizeForTour(m_TourData.pid);
		}
		else
		{
			m_downloadSize = Wezit.DataGrabber.Instance.GetDownloadSizeForAssets(Wezit.AssetsLoader.GetAssetsForTour(m_TourData.pid));
		}
		_downloadButtonsRoot.SetActive(m_downloadSize != 0);
		_startButton.gameObject.SetActive(m_downloadSize == 0);

		_downloadSizeText.gameObject.SetActive(m_downloadSize != 0);
		if (m_downloadSize > 0)
		{
			bool megaBytes = m_downloadSize / 1024f / 1024f > 1;
			PlayerManager.CurrentState.tourDownloadSize = m_downloadSize;
			_downloadSizeText.text = string.Format(megaBytes ? "{0:0.00} MB" : "{0:0.00} kB", megaBytes ? m_downloadSize / 1024f / 1024f : m_downloadSize / 1024f);
			_downloadSizeText.GetComponent<AccessibleText>().SetLabel(_downloadSizeText.text);
        }

		if (PlayerManager.Player.NumberOfSeeds > 0)
		{
			PlayerManager.Player.GetCurrentTourProgression().PercentOfCompletion = 1;
		}

		StartCoroutine(LayoutGroupRebuilder.Rebuild(_title.transform.parent.gameObject));
    }

	protected override void ResetViewContent()
	{
		base.ResetViewContent();
	}

	protected override void AddListeners()
	{
		base.AddListeners();

		_startButton.onClick.AddListener(OnStartButton);
		_startWithoutDownloadButton.onClick.AddListener(OnStartButton);
		_downloadButton.onClick.AddListener(OnDownloadButton);
	}

	protected override void RemoveListeners()
	{
		base.RemoveListeners();

		_startButton.onClick.RemoveAllListeners();
		_startWithoutDownloadButton.onClick.RemoveAllListeners();
		_downloadButton.onClick.RemoveAllListeners();
	}

	private void OnStartButton()
	{
		MatomoAnalyticsManager.Instance.RecordTourStarted(PlayerManager.CurrentState.CurrentTour.CleanedTitle, PlayerManager.CurrentState.CurrentTour.pid);
		SetState(PlayerManager.CurrentState.IsAudioDescription ? KioskState.LIST : KioskState.MAP);
	}

	private void OnDownloadButton()
	{
		SetState(KioskState.DOWNLOAD);
	}

	private void OnTourSpriteDownloaded(Sprite sprite)
	{
		PlayerManager.CurrentState.CurrentCharacterSprite = sprite;
	}
	#endregion Private
	#endregion Methods
}