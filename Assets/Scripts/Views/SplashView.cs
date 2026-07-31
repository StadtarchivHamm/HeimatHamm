using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;

public class SplashView : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private GameObject _root;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private VideoClip _defaultVideoClip;
    [SerializeField] private RawImage _videoRawImage;
    [SerializeField] private bool _loopVideo;
    [SerializeField] private AudioClip _jingle;
    #endregion
    #region Private
    private string m_videoName = "splash.mp4";
    #endregion
    #endregion

    #region Properties
    public bool VideoIsOver;
    #endregion

    #region Methods
    #region Monobehaviours
    private void Awake()
    {
        _root.SetActive(true);
        VideoIsOver = false;

        AppManager.Instance.OnLoadingOver.RemoveListener(OnLoadingOver);
        AppManager.Instance.OnLoadingOver.AddListener(OnLoadingOver);
        _videoRawImage.enabled = false;
        string videoPath = Path.Combine(Application.persistentDataPath, m_videoName);
        if (File.Exists(videoPath))
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		    	videoPath = "file://" + videoPath;
#endif
            _videoPlayer.url = videoPath;
            _videoPlayer.Prepare();
            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.loopPointReached += OnVideoLoopPointReached;
        }
        else
        {
            _videoPlayer.clip = _defaultVideoClip;
            _videoPlayer.Prepare();
            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.loopPointReached += OnVideoLoopPointReached;
        }

        BGMusicManager.Instance.PlayClip(_jingle);
    }
    #endregion
    #region Public
    public void Hide()
    {
        _root.SetActive(false);
        if (string.IsNullOrEmpty(PlayerManager.Player.Language))
        {
            AppManager.Instance.GoToState(KioskState.LANGUAGE_SELECTION);
        }
        else
        {
            StartCoroutine(AppManager.Instance.SetLanguage(LanguageExtensions.From(PlayerManager.Player.Language)));
            AppManager.Instance.GoToState(KioskState.HOME);
        }
    }
    #endregion
    #region Private
    private void OnVideoPrepared(VideoPlayer videoPlayer)
    {
        _videoRawImage.enabled = true;
        _videoRawImage.texture = _videoPlayer.targetTexture = new RenderTexture((int)_videoPlayer.width, (int)_videoPlayer.height, 32);
        _videoPlayer.isLooping = _loopVideo;
        _videoPlayer.Play();
    }

    private void OnVideoLoopPointReached(VideoPlayer videoPlayer)
    {
        VideoIsOver = true;

        if (AppManager.Instance.LoadingOver)
        {
            Hide();

            _videoPlayer.Stop();
            _videoPlayer.targetTexture.Release();
            Destroy(_videoPlayer.targetTexture);
        }
    }

    private void OnLoadingOver()
    {
        if (VideoIsOver)
        {
            Hide();

            _videoPlayer.Stop();
            _videoPlayer.targetTexture?.Release();
            Destroy(_videoPlayer.targetTexture);
        }
    }
    #endregion
    #endregion
}
