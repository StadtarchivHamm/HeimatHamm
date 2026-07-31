using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

/// <summary>
/// Set the image depending on a key from the Wezit Settings of the app. Changes with the current language.
/// </summary>
public class WezitSettingsImage : MonoBehaviour
{
	[SerializeField] private RawImage _image = null;
	[SerializeField] private string _settingsKey = "";
	[SerializeField] private WezitSourceTransformationEnum _transformation;
	[SerializeField] private bool _envelopeParent;
	private IDisposable _storeSubscription;
	private Language _currentLanguage = Language.none;
	private bool m_imageHasLoaded;

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

    private void OnEnable()
    {
		if (!m_imageHasLoaded && AppManager.Instance.LoadingOver)
		{
			UpdateImage();
		}
    }

    private void OnStoreStateChanged(State state)
	{
		if (state.Language != _currentLanguage)
		{
			_currentLanguage = state.Language;

			UpdateImage();
		}
	}

	private void OnLoadingOver()
	{
		_currentLanguage = StoreAccessor.State.Language;

		UpdateImage();

		if (_storeSubscription != null)
		{
			_storeSubscription.Dispose();
		}
		_storeSubscription = StoreAccessor.Subject.Subscribe((state) =>
		{
			OnStoreStateChanged(state);
		});
	}

	private void UpdateImage()
	{
		if (!gameObject.activeInHierarchy)
		{
			return;
		}

		if (!string.IsNullOrEmpty(_settingsKey))
		{
			if (_image == null)
			{
				Debug.LogWarning("Image object is null for image " + name);
				return;
			}
			Wezit.Settings.SetImageFromSetting(_image, this, _settingsKey, _currentLanguage, _transformation.ToString(), _envelopeParent);
			m_imageHasLoaded = true;

        }
	}
}