using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    #region Fields
    #region SerializeFields
    [SerializeField] private int _dropZoneIndex;
    #endregion
    #region Private
    private DragItem m_currentDragItem;
    #endregion
    #endregion

    #region Properties
    public DragItem CurrentDragItem { get => m_currentDragItem; }
    public Transform DropItemTargetTransform { get => transform; }
    public int DropZoneIndex { get =>  _dropZoneIndex; }

    public UnityEvent ItemDropped;
    public UnityEvent ItemRemoved;
    #endregion

    #region Methods
    #region Monobehaviours

    public void OnDrop(PointerEventData eventData)
    {
        DragItem dragItem = eventData.pointerDrag.gameObject.GetComponent<DragItem>();
        Debug.Log(name + " OnDrop " + dragItem?.name);
        if (dragItem != null)
        {
            if (m_currentDragItem != null && m_currentDragItem != dragItem)
            {
                m_currentDragItem.ResetDragItem();
                ItemRemoved?.Invoke();
            }

            if (dragItem.DragItemIndex == _dropZoneIndex)
            {
                ItemDropped?.Invoke();
                m_currentDragItem = dragItem;
                dragItem.OnValidDrop(this);
            }
            else
            {
                m_currentDragItem = null;
                dragItem.OnFailedDrop();
            }
        }
    }
    #endregion
    #region Public
    public void RemoveDragItem(DragItem dragItem)
    {
        if (dragItem == m_currentDragItem)
        {
            m_currentDragItem = null;

            ItemRemoved?.Invoke();
        }
    }

    #endregion
    #region Private

    #endregion
    #endregion
}
