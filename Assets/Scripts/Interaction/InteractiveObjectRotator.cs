using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveObjectRotator : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Transform _itemRoot;
    [SerializeField] private float _horizontalRotationSpeed = -0.5f;
    [SerializeField] private float _verticalRotationSpeed = 0.3f;
    [SerializeField] private float _verticalAngle = 30f; //The maximum vertical angle
    [SerializeField] private float _zoomSpeed = 0.3f;
    [SerializeField] private Vector2 _zoomLimits = new Vector2(.3f, 3f);
    [SerializeField] private float _dragSpeed = 0.3f;
    #endregion
    #region Private
    private Vector3 m_startPos;
    private Quaternion m_startRot;
    private float m_startScale;
    private float m_scale;

    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount == 0)
        {
            return;
        }

        if(Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Rotate(-touch.deltaPosition);
        }

        if(Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            TwoFingersBehaviour(touchZero, touchOne);
        }
    }

    private void OnDisable()
    {
        _itemRoot.localRotation = Quaternion.identity;
        _itemRoot.localPosition = Vector3.zero;
        _itemRoot.localScale = Vector3.one;
    }
    #endregion
    #region Public
    public void Init()
    {
        m_startPos = _itemRoot.localPosition;
        m_startRot = _itemRoot.localRotation;
        m_startScale = _itemRoot.localScale.x;
    }

    public void ResetPosRot()
    {
        _itemRoot.localRotation = m_startRot;
        _itemRoot.localPosition = m_startPos;
        _itemRoot.localScale = m_startScale * Vector3.one;
    }

    public void SetZoomLimits(Vector2 limits)
    {
        _zoomLimits = limits;
        m_scale = limits.x;
        _itemRoot.localScale = m_scale * Vector3.one;
    }

    public void SetZoomLimits(float min, float max)
    {
        SetZoomLimits(new Vector2(min, max));
    }
    #endregion
    #region Private
    private void TwoFingersBehaviour(Touch touchZero, Touch touchOne)
    {
        if (touchZero.deltaPosition.y * touchOne.deltaPosition.y > 0)
        {
            Drag((touchZero.deltaPosition.y > 0 ? 1 : -1) * touchZero.deltaPosition.magnitude);
        }
        else
        {
            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            Zoom(-deltaMagnitudeDiff);
        }
    }

    private void Rotate(Vector2 drag)
    {
        Vector3 newRot = _itemRoot.localEulerAngles;
        newRot = new Vector3(Utils.MathUtils.ClampAngle(newRot.x + drag.y * _verticalRotationSpeed, -_verticalAngle, _verticalAngle), 
                             newRot.y + drag.x * _horizontalRotationSpeed, 
                             newRot.z);
        _itemRoot.localEulerAngles = newRot;
    }

    private void Zoom(float value)
    {
        m_scale += value * _zoomSpeed;
        _itemRoot.localScale = Mathf.Clamp(m_scale, _zoomLimits.y, _zoomLimits.x) * Vector3.one;
        return;
    }

    private void Drag(float value)
    {
        _itemRoot.position += new Vector3(0, value * _dragSpeed, 0);
    }
    #endregion
    #endregion
}
