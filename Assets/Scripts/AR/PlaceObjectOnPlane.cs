using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class PlaceObjectOnPlane : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private GameObject _prefabToSpawn;
    [SerializeField] private Transform _objectRoot;
    #endregion

    #region Private
    private Camera m_aRCamera;
    private ARRaycastManager m_aRRaycastManager;
    private ARPlaneManager m_arPlaneManager;
    private List<ARRaycastHit> m_hits = new List<ARRaycastHit>();
    
    private Wezit.Poi m_poi;

    private GameObject m_spawnedObject;

    private bool m_placeOnPlane;
    private float m_distance;
    private float m_rotation;
    private float m_scale;

    private bool m_wasSeeingPlane;
    #endregion
    #endregion

    #region Public
    public UnityEvent<GameObject> ObjectPlaced = new UnityEvent<GameObject>();

    public UnityEvent<bool> IsSeeingPlane = new UnityEvent<bool>();
    #endregion

    #region Methods
    #region MonoBehaviours
    void Awake()
    {
        m_aRRaycastManager = GetComponent<ARRaycastManager>();
        m_arPlaneManager = GetComponent<ARPlaneManager>();
        m_aRCamera = GetComponent<XROrigin>().Camera;
        m_wasSeeingPlane = true;
    }

    private void LateUpdate()
    {
        if (m_placeOnPlane)
        {
            if (m_aRRaycastManager.Raycast(new Vector2(Screen.width / 2f, Screen.height / 2f), m_hits, TrackableType.PlaneWithinPolygon))
            {
                foreach (ARRaycastHit hit in m_hits)
                {
                    if (m_arPlaneManager.GetPlane(hit.trackableId).alignment != PlaneAlignment.HorizontalUp)
                    {
                        if (m_wasSeeingPlane)
                        {
                            m_wasSeeingPlane = false;
                            IsSeeingPlane?.Invoke(false);
                        }
                        return;
                    }

                    if (!m_wasSeeingPlane)
                    {
                        m_wasSeeingPlane = true;
                        IsSeeingPlane?.Invoke(true);
                    }
                }
            }
            else
            {
                if(m_wasSeeingPlane)
                {
                    m_wasSeeingPlane = false;
                    IsSeeingPlane?.Invoke(false);
                }
            }
        }
    }
    #endregion

    #region Public
    public void Inflate(Wezit.Poi poi)
    {
        m_poi = poi;

        m_placeOnPlane = !poi.type.ToLower().Contains("distance");

        float.TryParse(poi.location,
                       System.Globalization.NumberStyles.AllowDecimalPoint,
                       new System.Globalization.CultureInfo("en-US"),
                       out m_distance);
        m_distance = m_distance == 0 ? 2 : m_distance;

        float.TryParse(StringUtils.CleanFromWezit(m_poi.spatial),
                       System.Globalization.NumberStyles.AllowDecimalPoint,
                       new System.Globalization.CultureInfo("en-US"),
                       out m_rotation);

        float.TryParse(StringUtils.CleanFromWezit(m_poi.extent),
                       System.Globalization.NumberStyles.AllowDecimalPoint,
                       new System.Globalization.CultureInfo("en-US"),
                       out m_scale);
        m_scale = m_scale == 0 ? 1 : m_scale;
    }

    public async void PlaceAnimal()
    {
        if(m_placeOnPlane)
        {
            if (m_aRRaycastManager.Raycast(new Vector2(Screen.width / 2f, Screen.height / 2f), m_hits, TrackableType.PlaneWithinPolygon))
            {
                foreach (ARRaycastHit hit in m_hits)
                {
                    Pose pose = hit.pose;
                    if (m_arPlaneManager.GetPlane(hit.trackableId).alignment != PlaneAlignment.HorizontalUp)
                    {
                        return;
                    }

                    _objectRoot.position = pose.position;
                    _objectRoot.rotation = pose.rotation;
                    _objectRoot.localEulerAngles += new Vector3(0, m_rotation, 0);
                    _objectRoot.localScale = m_scale * Vector3.one;

                    Destroy(m_spawnedObject);
                    m_spawnedObject = await Utils.GLTFSpawner.SpawnGLTF(_objectRoot, m_poi);
                    ObjectPlaced?.Invoke(m_spawnedObject);
                }
            }
        }
        else
        {
            _objectRoot.position = m_aRCamera.transform.position + m_aRCamera.transform.forward * m_distance;
            _objectRoot.localEulerAngles = new Vector3(0, m_rotation, 0);
            _objectRoot.localScale = m_scale * Vector3.one;

            Destroy(m_spawnedObject);
            m_spawnedObject = await Utils.GLTFSpawner.SpawnGLTF(_objectRoot, m_poi);
            ObjectPlaced?.Invoke(m_spawnedObject);
        }

    }
    #endregion

    #region Private
    #endregion
    #endregion
}
