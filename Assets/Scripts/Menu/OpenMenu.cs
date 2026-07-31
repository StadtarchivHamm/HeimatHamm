using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpenMenu : MonoBehaviour
{
	#region Fields
	#region SerializeFields
	[SerializeField] private Transform _menuRoot;
	[SerializeField] private GameObject _uiRoot;
    [SerializeField] private Button _closeButton;
	[SerializeField] private List<LanguageButton> _languageButtons;
	[SerializeField] private List<MenuLink> _menuLinks;
	[SerializeField] private Button _debugButton;
	[SerializeField] private AudioSource _debugAudioSource;
	[SerializeField] private GameObject _debugIcon;
	#endregion

	#region Private
	private RectTransform m_menuRecttransform;
	private int m_debugCounter;
	private bool m_opening;
	#endregion
	#endregion Fields

	#region Methods
	#region MonoBehaviour
	private void OnEnable()
	{
		if (m_menuRecttransform == null)
		{
			m_menuRecttransform = _menuRoot.GetComponent<RectTransform>();
			m_menuRecttransform.anchorMin = m_menuRecttransform.anchorMax = Vector2.one * .5f;
			m_menuRecttransform.sizeDelta = new Vector2(Screen.width * 932 / Screen.height + 2, 932);
		}

		AddListeners();
	}

    private void OnDisable()
    {
        _menuRoot.localPosition = Vector3.left * _menuRoot.GetComponent<RectTransform>().sizeDelta.x;
	}
    #endregion MonoBehaviour

    #region Public
    public void Open()
    {
		m_opening = true;
		_uiRoot.SetActive(true);
		StartCoroutine(SlideOpen());
    }

	public void InitViewContentByLang(Language lang)
    {
		ResetViewContent();
		m_menuRecttransform = _menuRoot.GetComponent<RectTransform>();

		foreach (LanguageButton languageButton in _languageButtons)
		{
			languageButton.Init();
		}

		foreach (MenuLink menuLink in _menuLinks)
		{
			menuLink.Init();
		}

		AddListeners();
	}

    public void AddListeners()
	{
		RemoveListeners();
		_closeButton.onClick.AddListener(Close);

		foreach (LanguageButton languageButton in _languageButtons)
		{
			languageButton.LanguageButtonClicked.AddListener(OnLanguageButtonClicked);
		}

        foreach (MenuLink menuLink in _menuLinks)
        {
			menuLink.Init();
			menuLink.MenuLinkClicked.AddListener(OnMenuLinkClicked);
        }

		_debugButton.onClick.AddListener(OnDebugButton);
	}

	public void RemoveListeners()
	{
		_closeButton.onClick.RemoveAllListeners();

        foreach (LanguageButton languageButton in _languageButtons)
        {
			languageButton.LanguageButtonClicked.RemoveAllListeners();
		}

		foreach (MenuLink menuLink in _menuLinks)
		{
			menuLink.MenuLinkClicked.RemoveAllListeners();
		}

		_debugButton.onClick.RemoveAllListeners();
	}
	#endregion Public

	#region Private
	private void ResetViewContent()
	{
		Close();
        _debugIcon.SetActive(PlayerManager.CurrentState.IsInDevMode);
    }

	private void OnMenuLinkClicked(KioskState state, string url)
    {
		Close();
    }

	private void OnLanguageButtonClicked(Language language, bool save = true)
    {
		Close();
		if (save && language.ToString() != PlayerManager.Player.Language)
		{
			AppManager.Instance.GoToHome();
			StartCoroutine(AppManager.Instance.SetLanguage(language));
			PlayerManager.Player.SetLanguage(language);
		}
	}

	private IEnumerator SlideOpen()
    {
		float width = m_menuRecttransform.sizeDelta.x;
		m_menuRecttransform.anchoredPosition = width * Vector2.left;
		while(m_menuRecttransform.anchoredPosition.x < -0.1f && m_opening)
        {
			m_menuRecttransform.Translate(Mathf.Min(100, -m_menuRecttransform.anchoredPosition.x), 0, 0);
			yield return null;
        }
		m_menuRecttransform.anchoredPosition = Vector2.zero;
    }

	private void Close()
    {
		m_debugCounter = 0;
		m_opening = false;

		if (gameObject.activeInHierarchy)
        {
			StartCoroutine(SlideClosed());
        }
    }

	private IEnumerator SlideClosed(bool instantly = false)
	{
		float width = m_menuRecttransform.sizeDelta.x;
		while (m_menuRecttransform.anchoredPosition.x > -width || instantly)
		{
			m_menuRecttransform.Translate(-100, 0, 0);
			yield return null;
		}
		_uiRoot.SetActive(false);
	}

	private void OnDebugButton()
    {
		m_debugCounter += 1;
		Debug.Log("Debug click");
		if(m_debugCounter > 10)
        {
			PlayerManager.CurrentState.IsInDevMode = !PlayerManager.CurrentState.IsInDevMode;
			_debugIcon.SetActive(PlayerManager.CurrentState.IsInDevMode);
			m_debugCounter = 0;
			Handheld.Vibrate();
			_debugAudioSource.Play();
			Debug.LogWarning("Debug mode is " + (PlayerManager.CurrentState.IsInDevMode ? "active" : "inactive"));
		}
    }
	#endregion Private
	#endregion Methods
}
