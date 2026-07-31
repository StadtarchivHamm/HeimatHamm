using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformFollower : MonoBehaviour
{
    [SerializeField] private RectTransform _target;
    [SerializeField] private bool _followPosition = true;
    [SerializeField] private bool _followSize;

    private RectTransform m_thisRectTransform;

    private void Awake() 
    {
        m_thisRectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate() 
    {
        if (_followPosition)
        {
            m_thisRectTransform.position = _target.position;
        }

        if (_followSize)
        {
            m_thisRectTransform.sizeDelta = _target.sizeDelta;
        }
    }
}
