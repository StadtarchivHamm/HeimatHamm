using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ViewSelector : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _inactiveColor;
    [SerializeField] private Button _button;
    [Space]
    [Header("List")]
    [SerializeField] private Image _listBG;
    [Space]
    [Header("Map")]
    [SerializeField] private Image _mapBG;
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
    public void Init()
    {
        _mapBG.color = ViewManager.Instance.CurrentKioskState == KioskState.MAP ? _activeColor : _inactiveColor;
        _listBG.color = ViewManager.Instance.CurrentKioskState == KioskState.LIST ? _activeColor : _inactiveColor;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnButton);
    }
    #endregion
    #region Private
    private void OnButton()
    {
        AppManager.Instance.GoToState(ViewManager.Instance.CurrentKioskState == KioskState.MAP ? KioskState.LIST : KioskState.MAP);
    }
    #endregion
    #endregion
}
