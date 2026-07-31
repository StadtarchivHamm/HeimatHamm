using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Data;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class ResetView : BaseView
{
	#region Fields
	public static string TAG = "<color=orange>[HomeView]</color>";

	#region Serialize Fields
	[SerializeField] private Button _resetButton;
	[Space]
	[SerializeField] private GameObject _confirmationPopin;
	[SerializeField] private Button _confirmButton;
	[SerializeField] private Button _cancelButton;
	[SerializeField] private Button _closeButton;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
    #endregion Private m_Variables
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    #endregion Public

    #region Private
    protected override void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);
    }

    protected override void ResetViewContent()
    {
        base.ResetViewContent();

        _confirmationPopin.SetActive(false);
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        _resetButton.onClick.AddListener(OnResetButton);
        _confirmButton.onClick.AddListener(OnConfirmButton);
        _cancelButton.onClick.AddListener(OnCancelButton);
        _closeButton.onClick.AddListener(OnCancelButton);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        _resetButton.onClick.RemoveListener(OnResetButton);
        _confirmButton?.onClick.RemoveListener(OnConfirmButton);
        _cancelButton?.onClick.RemoveListener(OnCancelButton);
        _closeButton?.onClick.RemoveListener(OnCancelButton);
    }

    private void OnResetButton()
    {
        _confirmationPopin.SetActive(true);
    }

    private void OnConfirmButton()
    {
        PlayerManager.Player.Delete();
        _confirmationPopin.SetActive(false);
        AppManager.Instance.GoToState(KioskState.HOME);
    }

    private void OnCancelButton()
    {
        _confirmationPopin.SetActive(false);
    }
    #endregion Private
    #endregion Methods
}