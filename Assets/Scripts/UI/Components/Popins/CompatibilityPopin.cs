using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompatibilityPopin : Popin
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Button _continueButton;
    [SerializeField] private GameObject _buttonContainer;
    #endregion
    #region Private
    private string m_compatibleSettingKey = "language.screen.compatibility.positive.text";
    private string m_incompatibleSettingKey = "language.screen.compatibility.negative.text";
    private string m_detailsSettingKey = "language.screen.compatibility.details.text";
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public async void Inflate(bool open, MonoBehaviour monoBehaviour)
    {
        base.Inflate(open);

        _description.text = Wezit.Settings.GetSettingAsTaggedText(await CheckCompatibility.IsCompatible(monoBehaviour) ? m_compatibleSettingKey : m_incompatibleSettingKey);
        _continueButton.onClick.AddListener(OnContinueButton);
        _closeButton.onClick.AddListener(OnContinueButton);
    }

    public new void Open()
    {
        base.Open();

        StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_buttonContainer));
    }
    #endregion
    #region Private
    internal override void OnPopinButton()
    {
        base.OnPopinButton();
        _description.text = Wezit.Settings.GetSettingAsCleanedText(m_detailsSettingKey);
        StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(gameObject));
    }

    private void OnContinueButton()
    {
        Close(true);
        AppManager.Instance.GoToState(KioskState.HOME);
    }
    #endregion
    #endregion
}
