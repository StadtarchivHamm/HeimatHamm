using System;
using TMPro;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using Unity.Samples.ScreenReader;

/// <summary>
/// Set the label text depending on a key from the Wezit Settings of the app. Change with the current language.
/// </summary>
[RequireComponent(typeof(AccessibleElement))]
public class WezitSettingAccessibleElement : MonoBehaviour
{
	[SerializeField] private AccessibleElement _accessibleElement;
	[SerializeField] private string _settingsKey = "";
	[SerializeField] private Language _targetLanguage = Language.none;
	private IDisposable _storeSubscription;
	private Language _currentLanguage = Language.none;

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

	private void OnStoreStateChanged(State state)
	{
		if (_targetLanguage == Language.none && state.Language != _currentLanguage)
		{
			_currentLanguage = state.Language;
			UpdateLabel();
		}
	}

	private void OnLoadingOver()
	{
		_currentLanguage = _targetLanguage == Language.none ? StoreAccessor.State.Language : _targetLanguage;
		UpdateLabel();

		if (_storeSubscription != null)
		{
			_storeSubscription.Dispose();
		}
		_storeSubscription = StoreAccessor.Subject.Subscribe((state) =>
		{
			OnStoreStateChanged(state);
		});
	}

	private void UpdateLabel()
	{
		string label = Wezit.Settings.GetSettingAsCleanedText(_settingsKey, _currentLanguage);
        _accessibleElement.SetLabel(label);
        _accessibleElement.value = label;
		if (gameObject.activeInHierarchy)
		{
			this.DelayRefreshHierarchy();
		}
    }
}