using System.Collections.Generic;
using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private GameObject[] _flowers;
    [SerializeField] private float _spawnNormalOffset = 0.1f;
    [SerializeField] private LayerMask _touchableLayers = ~0; // All layers by default
    [SerializeField] private Camera _raycastCamera;
    #endregion
    #region Private
    // Tracks which finger IDs have already triggered a spawn this frame
    // so multi-finger taps don't fire twice on the same touch.
    private HashSet<int> m_processedTouches = new HashSet<int>();

    private string m_scaleSettingKey = "secret.poi.flowers.scale";
    private float m_scale;
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    private void Awake()
    {
        if (_raycastCamera == null)
        {
            _raycastCamera = Camera.main;
        }

        if (_raycastCamera == null)
        {
            Debug.LogError("[FlowerSpawner] No camera found. Assign one in the Inspector or tag your camera as MainCamera.");
        }

        if (_flowers == null || _flowers.Length == 0)
        {
            Debug.LogWarning("[TouchSpawner] No spawnPrefab assigned. Nothing will be spawned.");
        }

        m_scale = Wezit.Settings.GetSettingAsFloat(m_scaleSettingKey, 0.75f);
    }

    private void Update()
    {
        m_processedTouches.Clear();

#if UNITY_EDITOR
        // Editor / desktop fallback: use the mouse
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }
    #endregion
    #region Public

    #endregion
    #region Private

    // ---------------------------------------------------------------
    // Input handlers
    // ---------------------------------------------------------------

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Only react on the frame the finger first touches down
            if (touch.phase != TouchPhase.Began) continue;

            // Guard against duplicate processing
            if (m_processedTouches.Contains(touch.fingerId)) continue;
            m_processedTouches.Add(touch.fingerId);

            TrySpawnAt(touch.position);
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySpawnAt(Input.mousePosition);
        }
    }

    // ---------------------------------------------------------------
    // Core logic
    // ---------------------------------------------------------------

    /// <summary>
    /// Casts a ray from the camera through <paramref name="screenPosition"/>.
    /// If it hits a collider, spawns the prefab at the hit point.
    /// </summary>
    private void TrySpawnAt(Vector2 screenPosition)
    {
        if (_raycastCamera == null || _flowers == null || _flowers.Length == 0) return;

        Ray ray = _raycastCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _touchableLayers))
        {
            // No collider was hit � do nothing
            return;
        }

        SpawnObject(hit);
    }

    /// <summary>
    /// Instantiates the prefab at the raycast hit position,
    /// rotated to align with the surface normal.
    /// </summary>
    private void SpawnObject(RaycastHit hit)
    {
        if (hit.transform.parent.GetChild(1).childCount > 10)
        {
            return;
        }

        // Position: hit point + slight offset along the surface normal
        Vector3 spawnPosition = hit.point + hit.normal * _spawnNormalOffset;

        // Rotation: align the object's up-axis with the surface normal
        Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        int flowerIndex = Random.Range(0, _flowers.Length);

        GameObject spawned = Instantiate(_flowers[flowerIndex], spawnPosition, spawnRotation, hit.transform.parent.GetChild(1));
        spawned.transform.localScale = m_scale * Random.Range(0.8f, 1.2f) * Vector3.one;

        Debug.Log($"[TouchSpawner] Spawned '{spawned.name}' on '{hit.collider.name}' at {spawnPosition}");

        OnObjectSpawned(spawned, hit);
    }

    // ---------------------------------------------------------------
    // Extension point
    // ---------------------------------------------------------------

    /// <summary>
    /// Override or subscribe to this method to run custom logic
    /// immediately after an object is spawned.
    /// </summary>
    protected virtual void OnObjectSpawned(GameObject spawned, RaycastHit hit)
    {
        // Example: play a particle effect, sound, or animation here.
    }
    #endregion
    #endregion
}
