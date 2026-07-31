using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ContentSizeFitter))]
    public class ContentSizeFitterClamper : UIBehaviour, ILayoutSelfController
    {
        /// <summary>
        /// Maximum width of the container, set to -1 to not clamp
        /// </summary>
        [SerializeField] private float _maxWidth = -1;
        /// <summary>
        /// Maximum height of the container, set to -1 to not clamp
        /// </summary>
        [SerializeField] private float _maxHeight = -1;
        /// <summary>
        /// Maximum height of the container, set to -1 to not clamp
        /// </summary>
        [SerializeField] private float _sidesMinPadding = -1;
        private float m_maxWidthWithPadding;

        [System.NonSerialized] private RectTransform m_Rect;
        protected override void OnEnable()
        {
            SetDirty();

            if (_sidesMinPadding > 0)
            {
                Vector3[] worldCorners = new Vector3[4];
                rectTransform.parent.GetComponent<RectTransform>().GetWorldCorners(worldCorners);
                float canvasFactor = rectTransform.GetComponentInParent<Canvas>().transform.localScale.x;
                m_maxWidthWithPadding = worldCorners[2].x / canvasFactor - worldCorners[0].x / canvasFactor - _sidesMinPadding * 2;
            }
        }

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        public void SetLayoutHorizontal()
        {
            if (_maxWidth >= 0) rectTransform.sizeDelta = new Vector2(Mathf.Clamp(rectTransform.sizeDelta.x, Mathf.NegativeInfinity, _maxWidth), rectTransform.sizeDelta.y);
            if (_sidesMinPadding >= 0)
            {
                rectTransform.sizeDelta = new Vector2(Mathf.Clamp(rectTransform.sizeDelta.x, Mathf.NegativeInfinity, m_maxWidthWithPadding), rectTransform.sizeDelta.y);
            }
        }

        public void SetLayoutVertical()
        {
            if(_maxHeight >= 0) rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Clamp(rectTransform.sizeDelta.y, Mathf.NegativeInfinity, _maxHeight));
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif
    }
}