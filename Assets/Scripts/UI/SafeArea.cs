using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class SafeArea : MonoBehaviour
{
    [SerializeField] private bool _executeInEditMode = true;

    private RectTransform m_panel;
    private Rect m_lastSafeArea = new Rect(0, 0, 0, 0);

    public UnityEvent<float, float> SafeAreaApplied = new UnityEvent<float, float>();


    #region Monobehaviours
    private void Awake()
    {
        if (!Application.isPlaying && !_executeInEditMode) return;
        m_panel = GetComponent<RectTransform>();
        Refresh();
    }

    private void Update()
    {
        if (!Application.isPlaying && !_executeInEditMode) return;
        
        if (m_lastSafeArea == null)
        {
            m_lastSafeArea = GetSafeArea();
        }

        Refresh();
    }
    #endregion

    #region Public
    public void ForceRefresh()
    {
        ApplySafeArea(GetSafeArea());
    }
    #endregion

    #region Private
    private void Refresh()
    {
        Rect safeArea = GetSafeArea();

        if (safeArea != m_lastSafeArea)
            ApplySafeArea(safeArea);
    }

    private Rect GetSafeArea()
    {
        return Screen.safeArea;
    }

    private void ApplySafeArea(Rect rect)
    {
        m_lastSafeArea = rect;

        // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
        Vector2 anchorMin = rect.position;
        Vector2 anchorMax = rect.position + rect.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        m_panel.anchorMin = anchorMin;
        m_panel.anchorMax = anchorMax;

        SafeAreaApplied?.Invoke(rect.width / Screen.width, rect.height / Screen.height);
        Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            name, rect.x, rect.y, rect.width, rect.height, Screen.width, Screen.height);
    }
    #endregion
}