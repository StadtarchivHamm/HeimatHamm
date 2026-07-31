using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(RawImage), typeof(VideoPlayer), typeof(ImageAspectRatioSetter))]
public class WezitVideoPlayer : MonoBehaviour
{
	[SerializeField] private string _volumeSettingKey;
	private RawImage m_image;
	private VideoPlayer m_videoPlayer;
	private AspectRatioFitter m_aspectRatioFitter;

	private bool m_startWhenPrepared;
	private bool m_pauseWhenPrepared;
	private bool m_useChromaKey;

	public float VideoLength { get => (float)m_videoPlayer.length; }
	public UnityEvent<float> VideoPrepared = new UnityEvent<float>();
	public UnityEvent VideoEnded = new UnityEvent();
	public bool VideoIsOver;

	private void Awake()
	{
		VideoIsOver = false;
		m_image = GetComponent<RawImage>();
		m_videoPlayer = GetComponent<VideoPlayer>();
		m_aspectRatioFitter = GetComponent<AspectRatioFitter>();
	}

	public void Play()
	{
		m_videoPlayer.Play();
	}

	public void Pause()
	{
		m_videoPlayer.Pause();
	}

	public void Stop()
	{
		m_videoPlayer.Stop();
	}

	public async void PlayVideoFromPOI(Wezit.Node a_poi, string wezitSourceTransformation = "default", int index = 0, bool loop = true, bool startWhenPrepared = true, bool envelopeParent = true, bool useChromaKey = false, bool pauseWhenPrepared = false)
	{
		if (!string.IsNullOrEmpty(_volumeSettingKey))
		{
			m_videoPlayer.SetDirectAudioVolume(0, Wezit.Settings.GetSettingAsFloat(_volumeSettingKey));
		}

		m_startWhenPrepared = startWhenPrepared;
		m_pauseWhenPrepared = pauseWhenPrepared;
		m_useChromaKey = useChromaKey;

		if (a_poi == null)
		{
			Debug.LogWarning("Poi is null");
			return;
		}

		await a_poi.AreRelationsSet();

		if (a_poi.VideoRelations == null || a_poi.VideoRelations.Count == 0)
		{
			Debug.LogWarning("No video for poi " + a_poi.pid);
			return;
		}

		m_aspectRatioFitter.aspectMode = envelopeParent ? AspectRatioFitter.AspectMode.EnvelopeParent : AspectRatioFitter.AspectMode.FitInParent;
		m_videoPlayer.url = a_poi.VideoRelations[index].GetAssetSourceByTransformation(wezitSourceTransformation);
		m_videoPlayer.isLooping = loop;
		m_videoPlayer.Prepare();
		m_videoPlayer.prepareCompleted -= OnVideoPrepared;
		m_videoPlayer.prepareCompleted += OnVideoPrepared;
	}

	private void OnVideoPrepared(VideoPlayer videoPlayer)
	{
		VideoPrepared?.Invoke((float)videoPlayer.length);

		RenderTexture renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 32);
		m_videoPlayer.renderMode = VideoRenderMode.RenderTexture;
		m_videoPlayer.targetTexture = renderTexture;
		m_image.texture = renderTexture;

		if (m_useChromaKey)
		{
			m_image.material.SetTexture("_Texture", renderTexture);
		}

		if (m_startWhenPrepared || m_pauseWhenPrepared)
		{
			m_videoPlayer.Play();
			StartCoroutine(WaitForVideoEnd());

			if (m_pauseWhenPrepared)
			{
				m_videoPlayer.Pause();
			}
		}
	}

	private IEnumerator WaitForVideoEnd()
	{
		yield return new WaitForSeconds((float)m_videoPlayer.length);
		VideoIsOver = true;
		VideoEnded?.Invoke();
	}
}