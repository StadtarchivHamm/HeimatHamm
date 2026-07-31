using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Samples.ScreenReader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wezit;

public class SecretPoiListItem : PoiListItem
{
    #region Fields
    #region SerializeFields
    #endregion
    #region Private
    #endregion
    #endregion

    #region Properties
    public UnityEvent SecretPoiListItemClicked = new UnityEvent();
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion

    #region Public
    #endregion
    #region Private
    protected override void OnButtonClicked()
    {
        SecretPoiListItemClicked?.Invoke();

        base.OnButtonClicked();
    }
    #endregion
    #endregion
}
