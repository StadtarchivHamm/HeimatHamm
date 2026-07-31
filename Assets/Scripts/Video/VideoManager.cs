using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;
using System;

public class VideoManager : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private SafeArea _panelSafeArea;
    [Header("Video")]
	[SerializeField] private VideoPlayer _videoPlayer;
	[SerializeField] private RawImage _videoRawImage;
	[SerializeField] private SliderWithPointerEvents _slider;
	[SerializeField] private GraphicFader _graphicFader;
	[Header("Controls")]
	[SerializeField] private Button _closeButton;
	[SerializeField] private Button _playButton;
	[SerializeField] private Image _playButtonImage;
	[SerializeField] private Sprite _pauseIcon;
	[SerializeField] private Sprite _playIcon;
	[SerializeField] private Sprite _restartIcon;
	[Header("ControlsFading")]
	[SerializeField] private Button _screenButton;
	[SerializeField] private CanvasGroupFader _controlsFader;
    [Header("Text")]
	[SerializeField] private TMPro.TextMeshProUGUI _timeText;
	[SerializeField] private TMPro.TextMeshProUGUI _durationText;
	[SerializeField] private TMPro.TextMeshProUGUI _titleText;
	[Header("Audio")]
	[SerializeField] private AudioSource _audioSource;
    #endregion
    #region Private
    private RenderTexture m_videoRenderTexture;
    private const string TIMER_TEXT = "{0} / {1}";
    private ScreenOrientation m_userOrientation;
    private bool m_isSeeking;
	private bool m_isPaused;
	private Coroutine m_fadeOutCoroutine;
	private bool m_controlsAreHidden;
	#endregion
	#endregion

	#region Properties
	public UnityEvent<bool> VideoPlayerToggled = new UnityEvent<bool>();
	public UnityEvent VideoLoopPointReached = new UnityEvent();
	public UnityEvent<bool> VideoPaused = new UnityEvent<bool>();
	public UnityEvent<int> VideoPrepared = new UnityEvent<int>();
	public UnityEvent<float, float> VideoTimeChanged = new UnityEvent<float, float>();

	public VideoPlayer VideoPlayer
    {
		get => _videoPlayer;
    }
	#endregion

	#region Methods
	#region Monobehaviours
	private void OnEnable()
    {
        m_userOrientation = Screen.orientation;
        _graphicFader.StartFadingFromInit();
		VideoPlayerToggled?.Invoke(true);

		if(_videoPlayer.isPrepared)
        {
			OnVideoPrepared(_videoPlayer);
        }
		else
        {
			_videoPlayer.Prepare();

			_videoPlayer.prepareCompleted -= OnVideoPrepared;
			_videoPlayer.prepareCompleted += OnVideoPrepared;
        }
	}

	private void OnDisable()
    {
		if (m_videoRenderTexture != null)
		{
			m_videoRenderTexture.Release();
			Destroy(m_videoRenderTexture);
		}
    }
    #endregion

    #region Public
    public void Inflate(string videoSource, string title = "", AudioClip audioClip = null)
	{
		_videoRawImage.texture = null;
		_slider.SetValueWithoutNotify(0);
		if (m_videoRenderTexture != null)
		{
			m_videoRenderTexture.Release();
		}

		_videoPlayer.url = videoSource;
		_videoPlayer.isLooping = false;
		_videoPlayer.loopPointReached -= OnLoopPointReached;
		_videoPlayer.loopPointReached += OnLoopPointReached;
		_videoPlayer.Prepare();

		_titleText.text = "";
		_playButtonImage.sprite = _pauseIcon;
		_controlsFader.SetFadeValue(1);

		_videoPlayer.SetDirectAudioMute(0, audioClip != null);
		_audioSource.clip = audioClip;
		_audioSource.loop = false;

		AddListeners();
	}
	#endregion

	#region Private
	private void AddListeners()
    {
		RemoveListeners();

		_slider.onSliderPointerUp.AddListener(EndScrub);

		_slider.onSliderPointerDown.AddListener(BeginScrub);
		_slider.onValueChanged.AddListener(ScrubVideo);

		_playButton.onClick.AddListener(OnPlayButton);

		_screenButton.onClick.AddListener(OnScreenTap);

		_closeButton.onClick.AddListener(OnCloseButton);
	}

	private void RemoveListeners()
	{
		_playButton.onClick.RemoveAllListeners();

		_slider.onSliderPointerDown.RemoveAllListeners();
		_slider.onValueChanged.RemoveAllListeners();
		_slider.onSliderPointerUp.RemoveAllListeners();

		_screenButton.onClick.RemoveAllListeners();
		_closeButton.onClick.RemoveListener(OnCloseButton);
	}

	private void OnPlayButton()
	{
		if (_videoPlayer != null)
		{
			m_isPaused = false;
			if (!_videoPlayer.isPlaying)
			{
				if(_videoPlayer.time >= _videoPlayer.length)
                {
					_videoPlayer.time = 0;
					_slider.SetValueWithoutNotify(0);
					_audioSource.Play();
                }
				VideoPaused?.Invoke(false);
				_videoPlayer.Play();
				_audioSource.UnPause();
				_playButtonImage.sprite = _pauseIcon;
			}
			else
			{
				m_isPaused = true;
				_videoPlayer.Pause();
				_audioSource.Pause();
				VideoPaused?.Invoke(true);
				_playButtonImage.sprite = _playIcon;
			}
		}
	}

	private void BeginScrub()
	{
		//It is recommended to pause the player when seeking as otherwise,
		//you will continuously fight the VideoPlayer from playing and buffering frames.
		_videoPlayer.Pause();
		_audioSource.Pause();
		VideoPaused?.Invoke(true);

		//To know when the player has finished seeking      
		_videoPlayer.seekCompleted += PlayerSeekCompleted;
        m_isSeeking = false;
    }

	private void ScrubVideo(float value)
	{
		//If you are currently seeking there is no point to seek again.
		if (m_isSeeking)
			return;

		//Don't seek if the time between the slider value and the current player time is too small.
		//We will seek to the closest frame so if the delta is 0.00001f you will most likely seek the same frame.
		//Change the value to fit your use case.
		if (Mathf.Abs((float)_videoPlayer.time - value) < 0.01f)
			return;

		_videoPlayer.time = value;
		_audioSource.time = value;
		m_isSeeking = true;
	}

	public void EndScrub()
	{
		//You don't want random event when you are not using this script
		_videoPlayer.seekCompleted -= PlayerSeekCompleted;

		VideoTimeChanged?.Invoke((float)_videoPlayer.time, (float)_videoPlayer.length);

		if(!m_isPaused)
        {
			_videoPlayer.Play();
			_audioSource.UnPause();
			VideoPaused?.Invoke(false);
		}
	}

	private IEnumerator VideoProgressRoutine()
	{
		_slider.value = 0;
		while (!_videoPlayer.isPrepared)
        {
			yield return null;
		}
		m_fadeOutCoroutine = StartCoroutine(WaitAndFadeControls());

		_slider.minValue = 0;
		_slider.maxValue = (float)_videoPlayer.length;
		TimeSpan time = TimeSpan.FromSeconds(_videoPlayer.length);
		_durationText.text = time.ToString(@"mm\:ss");

		while (_videoPlayer.time < _videoPlayer.length)
		{
            _timeText.text = TimeSpan.FromSeconds(_videoPlayer.time).ToString(@"mm\:ss");
            _slider.SetValueWithoutNotify((float)_videoPlayer.time);
            yield return null;
		}
	}

	private void OnLoopPointReached(VideoPlayer videoPlayer)
	{
		_playButtonImage.sprite = _restartIcon;
		_videoPlayer.Stop();
		_audioSource.Stop();
		VideoLoopPointReached?.Invoke();

		if (m_fadeOutCoroutine != null)
		{
			StopCoroutine(m_fadeOutCoroutine);
			m_controlsAreHidden = false;
		}
		_controlsFader.SetFadeValue(1);
	}

	private void PlayerSeekCompleted(VideoPlayer source)
    {
		m_isSeeking = false;
    }

	private void OnCloseButton()
    {
		VideoLoopPointReached?.Invoke();
		OnClose();
    }

	private void OnVideoPrepared(VideoPlayer videoPlayer)
    {
        float ratio = (float)_videoPlayer.width / _videoPlayer.height;
        m_videoRenderTexture = new RenderTexture(ratio >= 1 ? 1920 : 1080, ratio > 1 ? 1080 : 1920, 32);
        _videoPlayer.targetTexture = m_videoRenderTexture;
        _videoRawImage.texture = m_videoRenderTexture;

        _videoPlayer.Play();
		_audioSource.Play();
		_playButtonImage.sprite = _pauseIcon;
        StartCoroutine(VideoProgressRoutine());
	}

	private void OnScreenTap()
    {
		if(m_controlsAreHidden)
        {
			if (m_fadeOutCoroutine != null)
			{
				StopCoroutine(m_fadeOutCoroutine);
				m_controlsAreHidden = false;
			}
			_controlsFader.SetFadeValue(1);

			m_fadeOutCoroutine = StartCoroutine(WaitAndFadeControls());
        }
		else
        {
			OnPlayButton();
        }

	}

	private IEnumerator WaitAndFadeControls()
    {
        while (!_videoPlayer.isPlaying)
        {
			yield return null;
        }

		yield return new WaitForSeconds(2f);
		while (!_videoPlayer.isPlaying)
		{
			yield return null;
		}
		m_controlsAreHidden = true;
		_controlsFader.StartFadingFromInit();
		m_fadeOutCoroutine = null;
    }

    private void OnClose()
    {
        VideoPlayerToggled?.Invoke(false);
        Screen.orientation = m_userOrientation;
        StartCoroutine(WaitForScreenPortrait());
        MenuManager.Instance.SetPreviousStatus();
    }

    private IEnumerator WaitForScreenPortrait()
    {
        while (Screen.orientation != m_userOrientation)
        {
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

		gameObject.SetActive(false);
    }
    #endregion
    #endregion
}
