using System;
using TMPro;
using UnityEngine;
using UniRx;

/// <summary>
/// Set the label text depending on a key from the Wezit Settings of the app. Change with the current language.
/// </summary>
public class WezitSettingsToggle : MonoBehaviour
{
	[SerializeField] private GameObject _objectToToggle = null;
	[SerializeField] private string _settingsKey = "";
	private IDisposable _storeSubscription;
	private Language _currentLanguage = Language.none;

	private void Awake()
	{
		if (!_objectToToggle) _objectToToggle = gameObject;
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
			bool value = Wezit.Settings.GetSettingAsBool(_settingsKey, _currentLanguage);
			_objectToToggle.SetActive(value);

			_currentLanguage = state.Language;
		}
	}

	private void OnLoadingOver()
	{
		bool value = Wezit.Settings.GetSettingAsBool(_settingsKey, Language.fr_FR);
		_objectToToggle.SetActive(value);

		_currentLanguage = StoreAccessor.State.Language;

		if (_storeSubscription != null)
		{
			_storeSubscription.Dispose();
		}
		_storeSubscription = StoreAccessor.Subject.Subscribe((state) =>
		{
			OnStoreStateChanged(state);
		});
	}
}