using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;
using UniRx;
using UnityEngine.SceneManagement;
using System.Linq;

public class LanguageSelectionView : BaseView
{
	#region Fields
	public GameObject splashView;
	private bool init;
	#region Serialize Fields
	[SerializeField] private List<LanguageButton> _languageButtons;
    #endregion Serialize Fields

    #region Public Variables
    #endregion Public Variables

    #region Private m_Variables
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
		init = true;

		BGMusicManager.Instance.FadeSoundOut(1);
		MenuManager.Instance.SetMenuStatus(MenuManager.MenuStatus.Hidden);

        AddListeners();

        List<Language> languages = new List<Language>();
		foreach (LanguageButton languageButton in _languageButtons)
		{
			languageButton.Init();
			if(languageButton.gameObject.activeInHierarchy)
            {
				languages.Add(languageButton.Language);
            }
		}

		if(languages.Count == 1)
        {
			OnLanguageButtonClicked(languages[0]);
        }
		else if(languages.Count == 0)
		{
			OnLanguageButtonClicked(Language.de);
        }
    }

	protected override void AddListeners()
	{
		base.AddListeners();

        foreach (LanguageButton languageButton in _languageButtons)
        {
			languageButton.LanguageButtonClicked.AddListener(OnLanguageButtonClicked);
        }
	}

    protected override void RemoveListeners()
	{
		base.RemoveListeners();

		foreach (LanguageButton languageButton in _languageButtons)
		{
			languageButton.LanguageButtonClicked.RemoveAllListeners();
		}
	}

	private async void OnLanguageButtonClicked(Language language, bool save = true)
    {
        if (save && language.ToString() != PlayerManager.Player.Language)
		{
			await StartCoroutine(AppManager.Instance.SetLanguage(language));
            PlayerManager.Player.SetLanguage(language);
        }
		SetState(KioskState.HOME);
    }

	#endregion Private

	#region Internals
	#endregion Internals
	#endregion Methods
}