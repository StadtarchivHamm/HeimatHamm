using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class MenuLink : MonoBehaviour
{
    #region Fields
    #region SerializeField
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Button _button;
    [SerializeField] private KioskState _kioskState;
    [Header("External url")]
    [SerializeField] private string _url;
    [SerializeField] private string _urlSettingKey;
    #endregion
    #region Private
    private bool m_initialized;
    #endregion
    #endregion

    #region Properties
    public UnityEvent<KioskState, string> MenuLinkClicked = new();
    #endregion

    #region Methods
    #region Monobehaviour

    private void OnEnable()
    {
        if (enabled)
        {
            if (AppManager.Instance.LoadingOver)
            {
                OnLoadingOver();
            }
            else
            {
                AppManager.Instance.OnLoadingOver.AddListener(OnLoadingOver);
            }
        }
    }
    #endregion
    #region Public
    public void Init()
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnButtonClicked);

        if (!string.IsNullOrEmpty(_urlSettingKey))
        {
            _url = Wezit.Settings.GetSetting(_urlSettingKey, ViewManager.Instance.CurrentLanguage);
        }

        m_initialized = true;
    }
    #endregion
    #region Private
    private void OnButtonClicked()
    {
        if (_kioskState != KioskState.NONE)
        {
            AppManager.Instance.GoToState(_kioskState);
        }
        else
        {
            if (!string.IsNullOrEmpty(_url))
            {
                Application.OpenURL(_url);
            }
        }

        MenuLinkClicked?.Invoke(_kioskState, _url);
    }

    private void OnLoadingOver()
    {
        if (!m_initialized && (_kioskState == KioskState.NONE))
        {
            Init();
        }
    }
    #endregion
    #endregion
}
