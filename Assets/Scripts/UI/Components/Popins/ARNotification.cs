using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ARNotification : Popin
{
    #region Fields
    #region SerializeFields
    [SerializeField] private TextMeshProUGUI _poiTitle;
    [SerializeField] private Button _moreInfoButton;
    [SerializeField] private RawImage _poiImage;
    [SerializeField] private AudioSource _notificationAudioSource;
    #endregion
    #region Private
    protected Wezit.Poi m_poi;
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public virtual void Inflate(bool open, Wezit.Poi poi)
    {
        if(poi == null)
        {
            Debug.LogWarning("POI is null");
            return;
        }

        base.Inflate(open);

        m_poi = poi;

        _poiTitle.text = m_poi.CleanedTitle;

        if (_moreInfoButton != null)
        {
            _moreInfoButton.onClick.RemoveAllListeners();
            _moreInfoButton.onClick.AddListener(OnContinueButton);
        }

        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(OnCloseButton);

        Utils.ImageUtils.LoadImage(_poiImage, this, m_poi.children.Find(x => x.tags.Contains(Tags.POI_LOCATION)));

        if (open && _notificationAudioSource != null)
        {
            _notificationAudioSource.Play();
        }
    }
    #endregion
    #region Private
    internal override void OnPopinButton()
    {
        base.OnPopinButton();

        PlayerManager.CurrentState.CurrentPoi = m_poi;
        AppManager.Instance.GoToState(KioskState.AR);
    }

    private void OnContinueButton()
    {
        PlayerManager.CurrentState.CurrentPoi = m_poi;
        PlayerManager.CurrentState.CurrentStationLocationPoi = m_poi.children.Find(x => x.tags.Contains(Tags.POI_LOCATION));
        AppManager.Instance.GoToState(KioskState.POI_DETAILS);
    }

    private void OnCloseButton()
    {
        Close();
    }
    #endregion
    #endregion
}
