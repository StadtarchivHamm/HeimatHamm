using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private enum Axis
    {
        X,
        Y,
        Z,
        All
    }

    #region Fields
    [SerializeField] private Axis _rotateAxis = Axis.All;
    [SerializeField] private Transform _cameraTransform;
    private Vector3 m_targetPosition;
    #endregion

    #region Methods
    #region Monobehaviour
    // Start is called before the first frame update
    void Start()
    {
        if(_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (_rotateAxis)
        {
            case Axis.X:
                m_targetPosition = new Vector3(transform.position.x, _cameraTransform.position.y, _cameraTransform.position.z);
                break;
            case Axis.Y:
                m_targetPosition = new Vector3(_cameraTransform.position.x, transform.position.y, _cameraTransform.position.z);
                break;
            case Axis.Z:
                m_targetPosition = new Vector3(_cameraTransform.position.x, _cameraTransform.position.y, transform.position.z);
                break;
            case Axis.All:
            default:
                m_targetPosition = _cameraTransform.position;
                break;
        }
        transform.LookAt(m_targetPosition);
    }
    #endregion
    #region Public
    public void Inflate(Camera camera)
    {
        _cameraTransform = camera.transform;
    }
    #endregion
    #endregion
}
