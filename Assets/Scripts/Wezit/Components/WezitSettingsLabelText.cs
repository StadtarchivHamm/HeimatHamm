using System;
using TMPro;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using Unity.Samples.ScreenReader;
using System.Collections.Generic;

/// <summary>
/// Set the label text depending on a key from the Wezit Settings of the app. Change with the current language.
/// </summary>
public class WezitSettingsLabelText : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _text = null;
	[SerializeField] private string _settingsKey = "";
	[SerializeField] private Language _targetLanguage = Language.none;
	[SerializeField] private bool _customTags = false;
	[SerializeField] private bool _replaceLineBreak = true;


    private IDisposable m_storeSubscription;
	private Language m_currentLanguage = Language.none;
	private Dictionary<Language, Language> m_easyLanguagePairs = new Dictionary<Language, Language>()
	{
		{ Language.de, Language.no },
		{ Language.en_EN, Language.nl },
	};

	private void OnEnable()
	{
		if (enabled)
		{
			if (AppManager.Instance.LoadingOver)
			{
				OnLoadingOver();
			}
			else
			{
				AppManager.Instance.OnLoadingOver.AddListener(OnLoadingOver);
			}
        }
        if (gameObject.activeInHierarchy)
        {
            this.DelayRefreshHierarchy();
        }
    }

    private void Awake()
    {
		if (_text == null)
		{
			_text = GetComponentInChildren<TextMeshProUGUI>();
		}
    }

    private void OnStoreStateChanged(State state)
	{
		if (_targetLanguage == Language.none && state.Language != m_currentLanguage)
		{
			m_currentLanguage = state.Language;
			UpdateText();
		}
	}

	private void OnLoadingOver()
	{
		m_currentLanguage = _targetLanguage == Language.none ? StoreAccessor.State.Language : _targetLanguage;
		UpdateText();

		if (m_storeSubscription != null)
		{
			m_storeSubscription.Dispose();
		}
		m_storeSubscription = StoreAccessor.Subject.Subscribe((state) =>
		{
			OnStoreStateChanged(state);
		});
	}

	private void UpdateText()
	{
		bool isEasyLanguage = PlayerManager.CurrentState.IsEasyToRead;
		Language language = _targetLanguage != Language.none ? _targetLanguage : isEasyLanguage ? m_easyLanguagePairs[m_currentLanguage] : m_currentLanguage;

        string label = _customTags ? Wezit.Settings.GetSettingAsTaggedText(_settingsKey, language, _replaceLineBreak) 
								   : Wezit.Settings.GetSettingAsCleanedText(_settingsKey, language, _replaceLineBreak);
		_text.text = label;
		
		if (TryGetComponent(out ContentSizeFitter contentSizeFitter))
		{
			Canvas.ForceUpdateCanvases();
		}
		
		if (TryGetComponent(out AccessibleElement accessibleElement))
		{
			accessibleElement.SetLabel(label);
			accessibleElement.value = label;

			if (gameObject.activeInHierarchy)
			{
				this.DelayRefreshHierarchy();
			}
        }
		
		if (GetComponentInParent<AccessibleButton>() != null)
		{
			GetComponentInParent<AccessibleButton>().SetLabel(label);
			GetComponentInParent<AccessibleButton>().value = label;

            if (gameObject.activeInHierarchy)
            {
                this.DelayRefreshHierarchy();
            }
        }
	}
}