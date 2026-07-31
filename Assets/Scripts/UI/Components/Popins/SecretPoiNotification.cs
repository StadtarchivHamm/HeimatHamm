using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecretPoiNotification : ARNotification
{
    #region Fields
    #region SerializeFields
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
    public override void Inflate(bool open, Wezit.Poi poi)
    {
        base.Inflate(open, poi);
    }
    #endregion
    #region Private
    internal override void OnPopinButton()
    {
        PlayerManager.CurrentState.CurrentPoi = m_poi;
        AppManager.Instance.GoToState(KioskState.SECRET_POI);
    }
    #endregion
    #endregion
}
