using System;
using TMPro;
using UnityEngine;
using UniRx;

/// <summary>
/// Set the label text depending on a key from the Wezit Settings of the app. Changes with the current language.
/// </summary>
public class WezitSettingsLabelTextWithBG : MonoBehaviour
{
	[SerializeField] private TextWithBackground _text = null;
	[SerializeField] private string _settingsKey = "";
	[SerializeField] private bool _customTags = false;
	private IDisposable _storeSubscription;
	private Language _currentLanguage = Language.none;

	private void Awake()
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

	private void OnStoreStateChanged(State state)
	{
		if (state.Language != _currentLanguage)
		{
			_currentLanguage = state.Language;

			UpdateText();
		}
	}

	private void OnLoadingOver()
	{
		_currentLanguage = StoreAccessor.State.Language;

		UpdateText();

		if (_storeSubscription != null)
		{
			_storeSubscription.Dispose();
		}
		_storeSubscription = StoreAccessor.Subject.Subscribe((state) =>
		{
			OnStoreStateChanged(state);
		});
	}

	private void UpdateText()
	{
		if (!string.IsNullOrEmpty(_settingsKey))
		{
			string label = _customTags ? Wezit.Settings.GetSettingAsTaggedText(_settingsKey, _currentLanguage) : Wezit.Settings.GetSettingAsCleanedText(_settingsKey, StoreAccessor.State.Language);
			_text.text = label;
		}
	}
}