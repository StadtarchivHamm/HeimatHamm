using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragZone : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private GameObject[] m_ObjectsToDeactivateOnDrop;

    #endregion
    #region Private

    #endregion
    #endregion

    #region Properties
    public Transform ItemTarget { get => transform; }
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public void OnItemDropped()
    {
        foreach (GameObject obj in m_ObjectsToDeactivateOnDrop)
        {
            obj.SetActive(false);
        }
    }

    public void OnResetItem()
    {
        foreach (GameObject obj in m_ObjectsToDeactivateOnDrop)
        {
            obj.SetActive(true);
        }
    }

    #endregion
    #region Private

    #endregion
    #endregion
}
