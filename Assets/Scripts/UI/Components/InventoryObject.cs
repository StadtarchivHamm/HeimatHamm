using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Utils;

public class InventoryObject : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Button _button;
    [SerializeField] private RawImage _thumbnail;
    [SerializeField] private TextMeshProUGUI _title;
    #endregion
    #region Private
    private Wezit.Poi m_poi;
    #endregion
    #endregion

    #region Properties
    public UnityEvent<Wezit.Poi> ObjectClicked = new UnityEvent<Wezit.Poi>();
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public InventoryObject Inflate(Wezit.Poi poi)
    {
        m_poi = poi;
        _button.onClick.AddListener(OnObjectClicked);

        _title.text = m_poi.CleanedTitle;
        ImageUtils.LoadImage(_thumbnail, this, m_poi, fillParent:false);

        return this;
    }
    #endregion
    #region Private
    private void OnObjectClicked()
    {
        ObjectClicked?.Invoke(m_poi);
    }
    #endregion
    #endregion
}
