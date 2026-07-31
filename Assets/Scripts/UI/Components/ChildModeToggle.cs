using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Toggle))]
public class ChildModeToggle : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Toggle _toggle;
    [SerializeField] private GameObject _adultRoot;
    #endregion
    #region Private

    #endregion
    #endregion

    #region Properties
    public UnityEvent<bool> ChildModeToggled = new UnityEvent<bool>();
    #endregion

    #region Methods
    #region Monobehaviours
    private void Awake()
    {
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);

        Init();
    }
    #endregion
    #region Public
    public void Init()
    {
        if (PlayerManager.CurrentState != null)
        {
            _toggle.SetIsOnWithoutNotify(PlayerManager.CurrentState.IsAudioDescription);
            _adultRoot.SetActive(!PlayerManager.CurrentState.IsAudioDescription);
        }
    }
    #endregion
    #region Private
    private void OnToggleValueChanged(bool isOn)
    {
        PlayerManager.CurrentState.IsAudioDescription = isOn;
        _adultRoot.SetActive(!isOn);
        ChildModeToggled?.Invoke(isOn);
    }
    #endregion
    #endregion
}
