using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class ARVideoPlayer : MonoBehaviour
{
    [Serializable]
    public struct ARVideoClip
    {
        public string id;
        public VideoClip videoClip;
    }

    #region Fields
    #region SerializeFields
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Billboard _billboard;
    [SerializeField] private RawImage _image;
    [SerializeField] private WezitVideoPlayer _wezitVideoPlayer;
    #endregion
    #region Private

    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods 
    #region Monobehaviours
    #endregion
    #region Public
    public void Inflate(Wezit.Poi poi, Camera arCamera)
    {
        _canvas.worldCamera = arCamera;

        _billboard.Inflate(arCamera);

        Color maskColor = Wezit.Settings.GetSettingAsColor("ar.past.video.chroma.key.color");
        maskColor = maskColor == Color.black ? StringUtils.GetStringAsColor("#0172FE") : maskColor;
        _image.material.SetColor("_MaskColor", maskColor);
        _wezitVideoPlayer.PlayVideoFromPOI(poi, envelopeParent:false, useChromaKey:true, startWhenPrepared:false, pauseWhenPrepared:true);
    }

    public void PlayVideo()
    {
        _wezitVideoPlayer.Play();
    }

    public void StopVideo()
    {
        _wezitVideoPlayer.Stop();
    }

    public void PauseVideo(bool isPaused)
    {
        if (isPaused)
        {
            _wezitVideoPlayer.Pause();
        }
        else
        {
            _wezitVideoPlayer.Play();
        }
    }
    #endregion
    #region Private
    #endregion
    #endregion
}
