using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToNorth : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;
    #endregion
    #region Private
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    // Start is called before the first frame update
    void Start()
    {
        // Compass is needed, precision is increased by having the location
        Utils.MapUtils.KeepRotating = true;
        Utils.MapUtils.StartRotationService(this);
        Utils.MapUtils.KeepLocating = true;
        Utils.MapUtils.StartLocationService(this);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = Utils.MapUtils.RotationToNorth * _rotationAxis;
    }
    #endregion
    #region Public

    #endregion
    #region Private

    #endregion
    #endregion
}
