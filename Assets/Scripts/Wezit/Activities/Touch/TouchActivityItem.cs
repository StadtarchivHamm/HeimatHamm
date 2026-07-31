using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TouchActivityItem : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _outlineRoot;
    #endregion
    #region Private
    private RectTransform m_rectTransform;
    #endregion
    #endregion

    #region Properties
    public UnityEvent TouchItemTouched = new UnityEvent();
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public TouchActivityItem Inflate(TouchItemModel touchItem, Vector2 parentSizeDelta)
    {
        _outlineRoot.SetActive(false);

        m_rectTransform = GetComponent<RectTransform>();
        m_rectTransform.anchoredPosition  = new Vector2(touchItem.point.relX * parentSizeDelta.x, -touchItem.point.relY * parentSizeDelta.y);
        m_rectTransform.sizeDelta = touchItem.point.circle * Vector2.one;

        _button.onClick.AddListener(OnButtonClicked);

        return this;
    }
    #endregion
    #region Private
    private void OnButtonClicked()
    {
        _outlineRoot.SetActive(true);
        _button.enabled = false;
        TouchItemTouched?.Invoke();
    }
    #endregion
    #endregion
}
