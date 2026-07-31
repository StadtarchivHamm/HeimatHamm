using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindingLayer : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private RectTransform _holeMask;
    [SerializeField] private GameObject _opaqueLayer;
    [SerializeField] private List<Graphic> _holeGraphics = new List<Graphic>();
    [SerializeField] private Animator _circleAnimator;
    [SerializeField] private float _hideDelay = 5f;
    #endregion

    #region Private
    private RectTransform m_rectTransform;
    private bool m_isTouching;
    private bool m_isHidden;
    private float m_waitTimeBeforeHide;
    #endregion
    #endregion Fields

    #region Methods
    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable() 
    {
        DisplayHole(false);
    }

    private void Update() 
    {
        if (Input.GetMouseButton(0) && RectTransformUtility.RectangleContainsScreenPoint(m_rectTransform, Input.mousePosition))
        {
            m_isTouching = true;
            Vector3 touchPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rectTransform, Input.mousePosition, null, out touchPosition);
            _holeMask.position = touchPosition;

            if (m_isHidden)
            {
                DisplayHole(true);
            }
        }
        else
        {
            if (!m_isHidden)
            {
                if (m_isTouching)
                {
                    m_isTouching = false;
                    m_waitTimeBeforeHide = 0f;
                }

                m_waitTimeBeforeHide += Time.deltaTime;

                if (m_waitTimeBeforeHide >= _hideDelay)
                {
                    DisplayHole(false);
                }
            }
        }
    }

    private void DisplayHole(bool show)
    {
        if (_circleAnimator != null)
        {
            _circleAnimator.SetBool("ShowOutline", show);
        }
        m_isHidden = !show;

        foreach (Graphic graphic in _holeGraphics)
        {
            Color graphicColor = graphic.color;
            graphicColor.a = show ? 1 : 0;
            graphic.color = graphicColor;
        }
    }
    #endregion Methods
}
