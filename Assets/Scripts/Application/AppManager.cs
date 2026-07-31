using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AppManager : Singleton<AppManager>
{
    #region Fields
    private static string TAG = "<color=red>[AppManager]</color>";

	private bool m_onStandByCompleteFlag;
	private bool m_loadingOver;
    #endregion

    #region Properties
	public bool LoadingOver { get => m_loadingOver; }
	public UnityEvent OnLoadingOver = new UnityEvent();
    #endregion

    #region Methods
    #region Public
    public void Init()
	{
		Debug.Log(TAG + " Init");
		m_loadingOver = false;

		PoiLocationStore.Init();
		Poi3DPositionStore.Init();
		CategoryStore.Init();

		ApplyConfigParams();
        PlayerManager.Init();
        StartApp();
	}

	public IEnumerator SetLanguage(Language language, bool force = false)
	{
		if (language != StoreAccessor.State.Language || force)
		{
			Debug.Log(TAG + " SetLanguage - " + language);
			StoreAccessor.Dispatch(Store.Kiosk.ActionCreator.SetLanguage(language));
			yield return null; // force 'one frame waiting' to apply language changes
		}
	}
    #endregion

    #region Internal
    internal void GoToState(KioskState kioskState)
	{
		if (StoreAccessor.State.KioskState != kioskState) StoreAccessor.Dispatch(Store.Kiosk.ActionCreator.SetState(kioskState));
	}

	internal void SelectPoi(Wezit.Poi _wzPoi)
	{
		DispatchPoiSelection(_wzPoi);
	}

	internal void UnselectPoi()
	{
		DispatchPoiSelection(null);
	}

	internal void GoToHome()
	{
		UnselectPoi();
		GoToState(KioskState.HOME);
	}

	internal IEnumerator GoToHome(bool forceLanguage = false)
	{
		UnselectPoi();
		yield return SetLanguage((Language)Enum.Parse(typeof(Language), AppConfig.ConfigModel.defaultLanguage), forceLanguage);
		GoToState(KioskState.HOME);
	}

	internal IEnumerator GoToHome(Language language, bool forceLanguage = false)
	{
		UnselectPoi();
		yield return SetLanguage(language, forceLanguage);
		GoToState(KioskState.HOME);
	}
    #endregion

    #region Private
	private void ApplyConfigParams()
	{
		Cursor.visible = AppConfig.ConfigModel.cursorVisible;

		if (AppConfig.ConfigModel.targetFrameRate > -1)
			Application.targetFrameRate = AppConfig.ConfigModel.targetFrameRate;

		ForceResolutionConfigParams();

		if (AppConfig.ConfigModel.resolutionSettings.force && AppConfig.ConfigModel.resolutionSettings.checkChanges) StartCoroutine(CheckResolutionCoroutine());
	}

	private void ForceResolutionConfigParams()
	{
		if (AppConfig.ConfigModel.resolutionSettings.force)
		{
			Screen.SetResolution(AppConfig.ConfigModel.resolutionSettings.targetWidth, AppConfig.ConfigModel.resolutionSettings.targetHeight, AppConfig.ConfigModel.resolutionSettings.fullscreen);
		}
	}

	private IEnumerator CheckResolutionCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);

			if ((Screen.width != AppConfig.ConfigModel.resolutionSettings.targetWidth) || (Screen.height != AppConfig.ConfigModel.resolutionSettings.targetHeight) || (Screen.fullScreen != AppConfig.ConfigModel.resolutionSettings.fullscreen))
			{
				ForceResolutionConfigParams();
			}
		}
	}

	private void StartApp()
	{
		m_loadingOver = true;
		OnLoadingOver.Invoke();
    }

	private void DispatchPoiSelection(Wezit.Poi _wzPoi)
	{
		StoreAccessor.Dispatch(Store.SelectedPoi.ActionCreator.Set(_wzPoi));
	}

	private void OnStandByComplete()
	{
		Debug.Log(TAG + "OnStandByComplete");

		if (!m_onStandByCompleteFlag)
		{
			m_onStandByCompleteFlag = true;

			StartCoroutine(GoToHome(false));
		}
	}

	private void OnStandByReset()
	{
        Debug.Log(TAG + "OnStandByReset");
        m_onStandByCompleteFlag = false;
	}
    #endregion
    #endregion
}
