using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    #region Fields
    #region SerializeFields
    [SerializeField] private RawImage _dragItemImage;

    private DragZone m_dragZone;
    private DropZone m_currentDropZone;
    private bool m_isDragging;
    private bool m_hasBeenDroppedInDropZone;
    private RectTransform m_rectTransform;
    private Camera m_uiCamera;
    private int m_fingerDragId;
    private CanvasGroup m_canvasGroup;
    #endregion
    #region Private

    #endregion
    #endregion

    #region Properties
    public RawImage DragItemImage {  get { return _dragItemImage; } }
    public int DragItemIndex;
    #endregion

    #region Methods
    #region Monobehaviours
    private void Awake()
    {
        m_rectTransform = transform as RectTransform;
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            m_uiCamera = rootCanvas.worldCamera;
        }
        m_canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (m_isDragging == true && m_hasBeenDroppedInDropZone == false)
        {
            if (Input.GetMouseButton(0) == false && (Input.touchCount == 0 || (Input.touchCount == 1 && Input.GetTouch(0).fingerId != m_fingerDragId)))
            {
                OnEndDrag(null);
            }
        }
    }

    private void OnDestroy()
    {
        m_isDragging = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetAsLastSibling();

        m_isDragging = false;
        m_hasBeenDroppedInDropZone = false;
        m_fingerDragId = eventData.pointerId;
        if (m_canvasGroup != null)
        {
            m_canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 dragPosition = Vector3.zero;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rectTransform, eventData.position, m_uiCamera, out dragPosition);
        m_rectTransform.position = dragPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 endDragPosition = m_rectTransform.position;
        m_isDragging = false;
        m_fingerDragId = 0;

        if (m_hasBeenDroppedInDropZone == false)
        {
            OnFailedDrop();
        }
    }

    // Called when another item is dropped on the item (on a drop zone for example)
    // Native Unity function, hence the lack of references
    public void OnDrop(PointerEventData eventData)
    {
        DragItem dragItem = eventData.pointerDrag.gameObject.GetComponent<DragItem>();
        if (dragItem != null && dragItem != this && m_currentDropZone != null)
        {
            m_currentDropZone.OnDrop(eventData);
            m_currentDropZone = null;
        }
    }
    #endregion
    #region Public
    public void Inflate(DragZone dragZone, int index)
    {
        m_dragZone = dragZone;
        transform.position = m_dragZone.transform.position;
        name = "Drag item #" + index;

        DragItemIndex = index;
    }

    public void ResetDragItem()
    {
        m_isDragging = false;
        m_hasBeenDroppedInDropZone = false;
        m_rectTransform.transform.position = m_dragZone.ItemTarget.position;
        m_dragZone.OnResetItem();

        if (m_canvasGroup != null)
        {
            m_canvasGroup.blocksRaycasts = true;
        }
    }

    public void OnValidDrop(DropZone targetDropZone)
    {
        m_hasBeenDroppedInDropZone = true;
        m_rectTransform.transform.position = targetDropZone.DropItemTargetTransform.position;

        if (DragItemIndex != targetDropZone.DropZoneIndex)
        {
            StartCoroutine(ReturnToDragZone(1));
            return;
        }

        m_dragZone.OnItemDropped();
        if (m_currentDropZone != null)
        {
            m_currentDropZone.RemoveDragItem(this);
        }
        m_currentDropZone = targetDropZone;

        if (m_canvasGroup != null)
        {
            m_canvasGroup.blocksRaycasts = true;
        }
    }

    public void OnFailedDrop()
    {
        m_hasBeenDroppedInDropZone = true;

        if (m_currentDropZone != null)
        {
            m_currentDropZone.RemoveDragItem(this);
        }

        StartCoroutine(ReturnToDragZone());
    }
    #endregion
    #region Private
    private IEnumerator ReturnToDragZone(float stayDuration = 0)
    {
        yield return new WaitForSeconds(stayDuration);

        Vector3 startPos = transform.position;
        Vector3 endPos = m_dragZone.ItemTarget.position;

        float timer = 0;
        float duration = .5f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, timer / duration);
            yield return null;
        }

        transform.position = endPos;

        m_dragZone.OnResetItem();

        if (m_currentDropZone != null)
        {
            m_currentDropZone.RemoveDragItem(this);
            m_currentDropZone = null;
        }

        if (m_canvasGroup != null)
        {
            m_canvasGroup.blocksRaycasts = true;
        }
    }
    #endregion
    #endregion
}
