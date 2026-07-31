using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class POINotification : Popin
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Button _continueButton;
    [SerializeField] private PoiListItem _poiListItem;
    [SerializeField] private AudioSource _notificationAudioSource;
    #endregion
    #region Private
    private Wezit.Poi m_poi;
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public void Inflate(bool open, Wezit.Poi poi)
    {
        if(poi == null)
        {
            Close();
            return;
        }

        base.Inflate(open);

        m_poi = poi;

        _continueButton.onClick.AddListener(OnContinueButton);

        _poiListItem.Inflate(poi, PlayerManager.Player.GetCurrentTourProgression().GetPoiProgression(poi.pid), this, false);

        if (open)
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
        PlayerManager.CurrentState.CurrentStationLocationPoi = m_poi.children.Find(x => x.tags.Contains(Tags.POI_LOCATION));
        AppManager.Instance.GoToState(KioskState.POI_DETAILS);
    }

    private void OnContinueButton()
    {
        Close(true);
    }
    #endregion
    #endregion
}
