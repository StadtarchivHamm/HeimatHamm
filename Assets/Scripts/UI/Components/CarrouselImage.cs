using System.Collections;
using System.Collections.Generic;
using Unity.Samples.ScreenReader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CarrouselImage : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private RawImage _image;
    [SerializeField] private AccessibleImage _accessibleImage;
    [SerializeField] private Button _fullScreenButton;
    #endregion
    #region Private
    private Wezit.Relation m_relation;
    #endregion
    #endregion

    #region Properties
    public UnityEvent<Wezit.Relation> CarrouselImageClicked = new UnityEvent<Wezit.Relation>();
    #endregion

    #region Methods
    #region Monobehaviour
    private void OnEnable()
    {

        this.DelayRefreshHierarchy();
    }
    #endregion
    #region Public
    public void Inflate(Wezit.Relation relation, MonoBehaviour monoBehaviour, bool enveloppeParent = false)
    {
        m_relation = relation;
        monoBehaviour.StartCoroutine(Utils.ImageUtils.SetImage(_image, relation.GetAssetSourceByTransformation(WezitSourceTransformation.default_base), "", enveloppeParent));

        if (!string.IsNullOrEmpty(relation.description))
        {
            if (_accessibleImage != null)
            {
                _accessibleImage.SetLabel(relation.CleanedDescription);
                _accessibleImage.value = relation.CleanedDescription;
                this.DelayRefreshHierarchy();
            }
        }

        _fullScreenButton.onClick.AddListener(OnFullScreenButton);
    }

    public void UpdateHierarchy(bool isShown)
    {
        if (_accessibleImage != null)
        {
            _accessibleImage.enabled = isShown;
        }
    }
    #endregion
    #region Private
    private void OnFullScreenButton()
    {
        CarrouselImageClicked?.Invoke(m_relation);
    }
    #endregion
    #endregion
}
