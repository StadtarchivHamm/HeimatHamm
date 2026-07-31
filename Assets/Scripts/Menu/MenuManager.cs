using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UniRx;
using System.Collections.Generic;

[System.Serializable]
public class UnityEventMenu : UnityEvent { };

public class MenuManager : Singleton<MenuManager>
{
	public enum MenuStatus
	{
		Hidden,
		Normal,
		BackButton,
		Home,
		Darken,
		OpenMenu,
		Map,
	}

	#region Fields
	public static string TAG = "<color=blue>[MenuManager]</color>";

	[SerializeField] private GameObject _uiRoot;
	[SerializeField] private GameObject _darken;
	[SerializeField] private GameObject _darkenBG;
	[Header("Top Menu")]
	[SerializeField] private Button _menuButton;
	[SerializeField] private GameObject _progressPanel;
	[SerializeField] private Image _progressBar;
	[SerializeField] private Button _backButton;
	[Header("Open Menu")]
	[SerializeField] private OpenMenu _openMenu;
	[SerializeField] private GameObject _openMenuUIRoot;

	private MenuStatus m_previousStatus;
	private MenuStatus m_currentStatus;
	private MenuStatus m_viewStatus;
	private Stack<KioskState> m_kioskStateHistory = new Stack<KioskState>();

	private IDisposable m_storeSubscription;
	private Language m_currentLanguage = Language.none;
	#endregion Fields

	#region Properties
	public Stack<KioskState> KioskStateHistory
    {
		get => m_kioskStateHistory;
    }

	public MenuStatus CurrentStatus
    {
		get => m_currentStatus;
    }

	public MenuStatus CurrentViewMenuStatus
    {
		get => m_viewStatus;
    }
    #endregion

    #region Methods
    #region MonoBehaviour
    private new void Awake()
	{
		base.Awake();
		AddListeners();
		SetMenuStatus(MenuStatus.Hidden);
		
		if (AppManager.Instance.LoadingOver)
		{
			OnLoadingOver();
		}
		else
		{
			AppManager.Instance.OnLoadingOver.AddListener(OnLoadingOver);
		}
	}
	#endregion MonoBehaviour

	#region Public
	public void AddListeners()
	{
		RemoveListeners();
		_menuButton.onClick.AddListener(OnOpenMenu);
		_backButton.onClick.AddListener(OnBackButton);

		m_currentLanguage = StoreAccessor.State.Language;

		if (m_storeSubscription != null)
		{
			m_storeSubscription.Dispose();
		}
		m_storeSubscription = StoreAccessor.Subject.Subscribe((state) =>
		{
			OnStoreStateChanged(state);
		});
	}

	public void RemoveListeners()
	{
		_menuButton.onClick.RemoveAllListeners();
		_backButton.onClick.RemoveAllListeners();
	}

	public void SetMenuStatus(MenuStatus a_status)
	{
		if (m_currentStatus != a_status && m_currentStatus != MenuStatus.Darken)
		{
			m_previousStatus = m_currentStatus;
			m_currentStatus = a_status;
		}

        _darken.SetActive(a_status == MenuStatus.Darken);
        _darkenBG.SetActive(a_status == MenuStatus.Darken);
        _uiRoot.SetActive(a_status != MenuStatus.Hidden);

        switch (a_status)
		{
			case MenuStatus.BackButton:
                _backButton.gameObject.SetActive(true);
				_menuButton.gameObject.SetActive(false);
                break;
			case MenuStatus.Home:
                _backButton.gameObject.SetActive(false);
                _menuButton.gameObject.SetActive(true);
                _progressPanel.SetActive(false);
                break;
			case MenuStatus.Map:
                _backButton.gameObject.SetActive(false);
                _menuButton.gameObject.SetActive(true);
				if (!_progressPanel.activeInHierarchy)
				{
                    _progressBar.fillAmount = PlayerManager.Player.GetCurrentTourProgression().PercentOfCompletion;
                }
                _progressPanel.SetActive(true);
                break;
			default:
				break;
		}

		StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_uiRoot));
	}

	public void SetPreviousStatus()
	{
		SetMenuStatus(m_previousStatus);
	}

	public void Return()
    {
		OnBackButton();
    }

	public void ToggleInteractivity(bool isInteractive)
	{
		_backButton.interactable = isInteractive;
		_menuButton.interactable = isInteractive;
	}

	public void UpdateProgress()
	{
        //_progressBar.fillAmount = PlayerManager.Player.NumberOfSeeds / (float)PlayerManager.Player.MaxNumberOfSeeds;
        _progressBar.fillAmount = PlayerManager.Player.GetCurrentTourProgression().PercentOfCompletion;
    }
	#endregion Public

	#region Private
	private void InitViewContentByLang(Language language)
	{
		ResetViewContent();

		_openMenu.InitViewContentByLang(language);
    }

	private void ResetViewContent()
	{
		_openMenuUIRoot.SetActive(false);
		_openMenu.gameObject.SetActive(true);
	}

	private void OnLoadingOver()
	{
		InitViewContentByLang(StoreAccessor.State.Language);
		AddListeners();
	}

	private void OnStoreStateChanged(State state)
	{
		if (state.Language != m_currentLanguage)
		{
			m_currentLanguage = state.Language;
			InitViewContentByLang(state.Language);
		}
		SetMenuStateDependingOnKioskState(state.KioskState);
    }

	private void SetMenuStateDependingOnKioskState(KioskState kioskState)
    {
		switch(kioskState)
        {
			case KioskState.NONE:
			case KioskState.SPLASH:
			case KioskState.LANGUAGE_SELECTION:
				m_viewStatus = MenuStatus.Hidden;
				break;
			case KioskState.HOME:
				m_viewStatus = MenuStatus.Home;
				break;
			case KioskState.MAP:
				m_viewStatus = MenuStatus.Map;
				break;
			case KioskState.LIST:
                m_viewStatus = PlayerManager.CurrentState.IsAudioDescription ? MenuStatus.Map : MenuStatus.BackButton;
				break;
            case KioskState.POI_DETAILS:
			case KioskState.AR:
			case KioskState.MINIGAME_AR:
			case KioskState.MINIGAME_QUIZ:
			case KioskState.MINIGAME_DRAGDROP:
			case KioskState.MINIGAME_SLIDING_PUZZLE:
			case KioskState.MINIGAME_TOUCH:
			case KioskState.MINIGAME_DIAPORAMA:
			case KioskState.ACCESSIBILITY:
			case KioskState.INVENTORY:
			case KioskState.TUTORIAL:
			case KioskState.FAQ:
			case KioskState.RESET:
			case KioskState.DATA_PROTECTION:
			case KioskState.LEGAL_NOTICE:
			case KioskState.TERMS_OF_USE:
			case KioskState.CONTACT:
			case KioskState.CREDITS:
				m_viewStatus = MenuStatus.BackButton;
				break;
			default:
				m_viewStatus = MenuStatus.BackButton;
				break;
		}
		SetMenuStatus(m_viewStatus);
    }

	private void OnBackButton()
	{
		if(m_kioskStateHistory.Count == 0)
        {
			m_kioskStateHistory.Push(KioskState.HOME);
        }

		KioskState currentKioskState = m_kioskStateHistory.Pop();
		KioskState previousKioskState = m_kioskStateHistory.Pop();

		AppManager.Instance.GoToState(previousKioskState);
	}

	private void OnOpenMenu()
	{
		_openMenu.Open();
	}
	#endregion Private
	#endregion Methods
}
