using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Wezit;

public class FullscreenImageViewer : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [Header("Fullscreen")]
    [SerializeField] private GameObject _fullscreenRoot;
    [SerializeField] private PinchableScrollRect _pinchableScrollRect;
    [SerializeField] private RawImage _fullscreenImage;
    [SerializeField] private Button _fullscreenClose;
    [SerializeField] private TextMeshProUGUI _fullscreenLegend;
    [SerializeField] private TextMeshProUGUI _fullscreenCopyright;
    #endregion
    #region Private

    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    private void OnEnable()
    {
        _fullscreenClose.onClick.RemoveAllListeners();
        _fullscreenClose.onClick.AddListener(OnFullscreenClose);
    }
    #endregion
    #region Public
    public void Inflate(Relation relation, bool enable = true)
    {
        Toggle(enable);

        if (_pinchableScrollRect)
        {
            _pinchableScrollRect.Init(true);
        }
        if (_fullscreenImage)
        {
            StartCoroutine(ImageUtils.SetImage(_fullscreenImage, relation.GetAssetSourceByTransformation(WezitSourceTransformation.default_base), "", false));
        }
        if (_fullscreenLegend)
        {
            _fullscreenLegend.text = relation.CleanedDescription;
            _fullscreenCopyright.text = relation.CleanedSubject;
        }
    }

    public void Toggle(bool isOn)
    {
        _fullscreenRoot.SetActive(isOn);
    }
    #endregion
    #region Private

    private void OnFullscreenClose()
    {
        if (_fullscreenRoot)
        {
            _fullscreenRoot.SetActive(false);
        }

        if (_fullscreenImage)
        {
            ImageUtils.ResetImage(_fullscreenImage);
        }
    }

    #endregion
    #endregion
}
