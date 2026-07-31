using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UniRx;

/// <summary>
/// Set the video depending on a key from the Wezit Settings of the app. Changes with the current language.
/// </summary>
[RequireComponent(typeof(RawImage), typeof(VideoPlayer), typeof(ImageAspectRatioSetter))]
public class WezitSettingsVideo : MonoBehaviour
{
	[SerializeField] private string _settingsKey = "";
	[SerializeField] private WezitSourceTransformationEnum _transformation;
	[SerializeField] private bool _loop = true;
	[SerializeField] private bool _startWhenPrepared = true;
	[SerializeField] private bool _envelopeParent = true;
	private IDisposable m_storeSubscription;
	private Language m_currentLanguage = Language.none;
	private RawImage m_image;
	private VideoPlayer m_videoPlayer;
	private AspectRatioFitter m_aspectRatioFitter;

	private void Awake()
	{
		m_image = GetComponent<RawImage>();
		m_videoPlayer = GetComponent<VideoPlayer>();
		m_aspectRatioFitter = GetComponent<AspectRatioFitter>();

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
		if (state.Language != m_currentLanguage)
		{
			m_currentLanguage = state.Language;

			UpdateVideo();
		}
	}

	private void OnLoadingOver()
	{
		m_currentLanguage = StoreAccessor.State.Language;

		UpdateVideo();

		if (m_storeSubscription != null)
		{
			m_storeSubscription.Dispose();
		}
		m_storeSubscription = StoreAccessor.Subject.Subscribe((state) =>
		{
			OnStoreStateChanged(state);
		});
	}

	private void UpdateVideo()
	{
		if (!string.IsNullOrEmpty(_settingsKey))
		{
			if (m_image == null || m_videoPlayer == null)
			{
				Debug.LogWarning("Image object or video player is null for image " + name);
				return;
			}

			m_aspectRatioFitter.aspectMode = _envelopeParent ? AspectRatioFitter.AspectMode.EnvelopeParent : AspectRatioFitter.AspectMode.FitInParent;
			m_videoPlayer.url = Wezit.Settings.GetSettingAsAssetSourceByTransformation(_settingsKey, m_currentLanguage, _transformation.ToString());
			m_videoPlayer.isLooping = _loop;
			m_videoPlayer.Prepare();
			m_videoPlayer.prepareCompleted -= OnVideoPrepared;
			m_videoPlayer.prepareCompleted += OnVideoPrepared;
		}
	}

	private void OnVideoPrepared(VideoPlayer videoPlayer)
    {
		RenderTexture renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 32);
		m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		m_videoPlayer.targetTexture = renderTexture;
		m_image.texture = renderTexture;

        if (_startWhenPrepared)
        {
			m_videoPlayer.Play();
        }
    }
}