using System.Collections;
using TMPro;
using UniRx.Async;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Utils;
using Wezit;

public class SecretPoiView : BaseView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private Button _closeButton;
	[SerializeField] private Popin _closeWarningPopin;
	[Header("Intro")]
	[SerializeField] private GameObject _introRoot;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
	private SecretPoiARManager m_arManager;
	private Poi m_poi;
    private Poi m_stationPositionPoi;
    private Poi m_grassPatchesPoi;
    private bool m_aRIsLoaded;
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
            SceneManager.UnloadScene(4);

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

        m_grassPatchesPoi = m_poi.children.Find(child => child.tags.Contains(Tags.GRASS_PATCHES));
        if (m_grassPatchesPoi == null)
        {
            Debug.LogError("No grass patches POI");
        }

        _closeWarningPopin.Inflate(false);

        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(4, LoadSceneMode.Additive);
		sceneLoading.completed += OnArLoaded;
	}

    protected override void ResetViewContent()
	{
		base.ResetViewContent();

		_introRoot.SetActive(false);

        _closeWarningPopin.Close(false);
    }

    protected override void AddListeners()
	{
		base.AddListeners();

		_closeButton.onClick.AddListener(OnCloseButton);
		_closeWarningPopin.PopinButtonClicked.AddListener(OnWarningPopinConfirm);
		_closeWarningPopin.PopinSecondaryButtonClicked.AddListener(OnWarningPopinCancel);
    }

    protected override void RemoveListeners()
	{
		base.RemoveListeners();

		_closeButton.onClick.RemoveAllListeners();
		_closeWarningPopin.PopinButtonClicked.RemoveAllListeners();
		_closeWarningPopin.PopinSecondaryButtonClicked.RemoveAllListeners();
    }

	private async void OnArLoaded(AsyncOperation loadingAsyncOperation)
	{
		await UniTask.Yield();
		m_aRIsLoaded = true;
		m_arManager = FindFirstObjectByType<SecretPoiARManager>();

		m_arManager.Inflate(m_poi, m_stationPositionPoi, m_grassPatchesPoi);

		m_arManager.MarkerDetected.RemoveAllListeners();
		m_arManager.MarkerDetected.AddListener(OnMarkerDetected);
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
        _introRoot.SetActive(true);
    }
    #endregion Private
    #endregion Methods
}