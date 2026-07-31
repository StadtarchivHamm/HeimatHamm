using System;
using UnityEngine;
using Wezit;
using UnityEngine.Events;

/// <summary>
/// Base view for navigation in a Wezit-related app.
/// A view generally corresponds to a Wezit Poi.
/// </summary>
public abstract class BaseView : MonoBehaviour
{
	#region Fields
	#region Public Variables
	public KioskState KioskState;
	#endregion Public Variables
	
	#region Serialize Fields
	[SerializeField] protected GameObject _interfaceRoot;
	[SerializeField] protected bool _showViewOnInit;
	[Tooltip("Here you can setup actions when the view becomes visible (Fade In, etc)")]
	[SerializeField] protected UnityEvent _onInterfaceVisible = null;
	[Tooltip("Here you can setup actions when the view becomes hidden (Fade Out, etc)")]
	[SerializeField] protected UnityEvent _onInterfaceHidden = null;
	#endregion Serialize Fields

	#region Private m_Variables
	private CanvasGroupFader m_fader;
	protected IDisposable m_storeSubscription;
	protected bool m_isActive;
	private BaseView m_viewToHideWhenFadeIn = null;
	#endregion Private m_Variables
	#endregion Fields

	#region Properties
	public bool IsActive { get { return m_isActive; } }
	internal CanvasGroupFader Fader { get => m_fader; }
	#endregion Properties

	#region Methods
	#region Public
	public void SetState(KioskState state)
	{
		Debug.Log("BaseView - SetState - " + state);
		AppManager.Instance.GoToState(state);
	}

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}

	public virtual void InitView()
    {
		SetInterfaceVisible(_showViewOnInit);
	}

	public virtual void ShowView()
	{
		SetInterfaceVisible(true);
		InitViewContentByLang(StoreAccessor.State.Language);
		MenuManager.Instance.KioskStateHistory.Push(KioskState);
		AddListeners();
	}

	public virtual void HideView()
	{
		RemoveListeners();
		SetInterfaceVisible(false);
	}

	public virtual void OnLanguageUpdated(Language language)
    {
		if (m_isActive && AppManager.Instance.LoadingOver)
		{
			InitViewContentByLang(language);
		}
	}

	public virtual void OnSelectedPoi(Poi selectedPoi) { }

	public void SetInterfaceVisible(bool visible)
	{
		m_isActive = visible;
		m_viewToHideWhenFadeIn = null;

		if (_interfaceRoot)
		{
			_interfaceRoot.SetActive(visible);

			if (visible)
			{
				if (_onInterfaceVisible != null)
				{
					_onInterfaceVisible.Invoke();
					m_viewToHideWhenFadeIn = ViewManager.Instance.GetOldView();

					if (m_fader)
					{
						m_fader.StartFadingFromInit();
					}
					else
					{
						OnFadeEnd();
					}
					ViewManager.Instance.PrepareHideOldView();
				}
			}
			else
			{
				if (_onInterfaceHidden != null)
				{
					_onInterfaceHidden.Invoke();
				}
			}
		}
	}

	public virtual void PrepareHideView() { }
	#endregion Public

	#region MonoBehaviour
	protected virtual void Awake()
	{
		m_fader = _interfaceRoot.GetComponentInChildren<CanvasGroupFader>();
		AppManager.Instance.OnLoadingOver.AddListener(OnLoadingOver);
	}
	#endregion MonoBehaviour

	#region Private
	private void OnLoadingOver()
	{
		if (m_fader)
		{
			if (AppConfig.ConfigModel.screenFadeTime > 0)
			{
				m_fader.FadeTime = AppConfig.ConfigModel.screenFadeTime;
			}
			m_fader.OnFadeEnd.AddListener(OnFadeEnd);
		}
	}

	private void OnFadeEnd()
	{
		OnFadeEndView();

		if (m_viewToHideWhenFadeIn != null)
		{
			m_viewToHideWhenFadeIn.HideView();
		}
		else
		{
			ViewManager.Instance.HideOldView();
		}
	}
	#endregion Private

	#region Internals
	protected virtual void OnFadeEndView() { }

	protected virtual void InitViewContentByLang(Language language)
    {
		if (language == Language.none)
		{
			return;
		}
		ResetViewContent();
		AddListeners();
    }

	protected virtual void ResetViewContent()
    {
		RemoveListeners();
	}

	protected virtual void AddListeners()
	{
		RemoveListeners();
	}

	protected virtual void RemoveListeners()
	{

	}
	#endregion Internals
	#endregion Methods
}
