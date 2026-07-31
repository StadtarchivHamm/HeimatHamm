using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Wezit;

public class SecretPoiARManager : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Camera _arCamera;
    [SerializeField] private ARTrackedImageManager _aRTrackedImageManager;
    [SerializeField] private ARSession _arSession;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject _uiRoot;
    [SerializeField] private Transform _objectRoot;
    [SerializeField] private List<GrassPatch> _grassPatchPrefabs;
    [Header("Tracking Stabilization")]
    [SerializeField, Min(1)] private int _requiredTrackedFramesBeforePlacement = 5;
    [Header("Occlusion management")]
    [SerializeField] private ARPlaneManager _planeManager;
    [SerializeField] private GameObject _shadowCasterARPlane;
    [SerializeField] private GameObject _occlusionARPlane;
    #endregion

    #region Private
    private string m_useOcclusionPlanesSettingKey = "ar.use.occlusion.toggle";

    private Poi m_poi;

    private Poi m_stationLocationPoi;
    private ARLocation.Location m_poiARLocation;
    private float m_angleToNorth;
    private float m_heightOffset;

    private Poi m_grassPatchesPoi;
    private List<GrassPatch> m_grassPatches = new List<GrassPatch>();

    private Coroutine m_markerStabilizationCoroutine;
    private ARTrackedImage m_candidateTrackedImage;
    private bool m_markerImageHasBeenDownloaded;
    private bool m_markerHasBeenDetected;
    private Transform m_detectedImageTransform;
    #endregion
    #endregion

    #region Properties
    public UnityEvent MarkerDetected = new UnityEvent();
    #endregion

    #region Methods
    #region Monobehaviours
    void OnEnable()
    {
        _aRTrackedImageManager.trackablesChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        _aRTrackedImageManager.trackablesChanged.RemoveListener(OnChanged);


        m_markerHasBeenDetected = false;
        m_markerImageHasBeenDownloaded = false;
    }

    private void Start()
    {
        MatomoAnalyticsManager.Instance.RecordSecretARSessionStarted();
    }

    IEnumerator ResetARSession()
    {
        _arSession.Reset(); // Forces ARKit to fully reinitialize
        yield return new WaitUntil(() => ARSession.state == ARSessionState.SessionTracking);
    }
    #endregion

    #region Public
    public async void Inflate(Poi currentPoi, Poi stationLocationPoi, Poi grassPatchesPoi)
    {
        _planeManager.planePrefab = Settings.GetSettingAsBool(m_useOcclusionPlanesSettingKey) ? _occlusionARPlane : _shadowCasterARPlane;

        m_poi = currentPoi;
        _objectRoot.gameObject.SetActive(false);

        m_stationLocationPoi = stationLocationPoi;
        m_poiARLocation = new ARLocation.Location(PoiLocationStore.GetPoiLocationById(m_stationLocationPoi?.pid));

        m_angleToNorth = StringUtils.GetStringAsFloat(m_stationLocationPoi.spatial);
        m_heightOffset = StringUtils.GetStringAsFloat(m_stationLocationPoi.extent);

        ToggleUI(true);

        await StartCoroutine(ResetARSession());
        _aRTrackedImageManager.enabled = false;

        await m_stationLocationPoi.AreRelationsSet();
        if (m_stationLocationPoi.RefPictureRelations.Count > 0)
        {
            if (PlayerManager.CurrentState.RuntimeReferenceImageLibrary == null)
            {
                PlayerManager.CurrentState.RuntimeReferenceImageLibrary = _aRTrackedImageManager.CreateRuntimeLibrary();
            }
            _aRTrackedImageManager.referenceLibrary = PlayerManager.CurrentState.RuntimeReferenceImageLibrary;
            _aRTrackedImageManager.enabled = true;
            StartCoroutine(TextureAndSpriteUtils.GetTextureFromSource(m_stationLocationPoi.RefPictureRelations[0].GetAssetSourceByTransformation(WezitSourceTransformation.default_base), OnQRCodeDownloaded));
        }

        m_grassPatchesPoi = grassPatchesPoi;

        if (m_markerHasBeenDetected)
        {
            OnMarkerDetected();
        }
    }

    public void ToggleUI(bool isOn)
    {
        _uiRoot.SetActive(isOn);
    }
    #endregion

    #region Private
    private void OnQRCodeDownloaded(Texture2D texture2D)
    {
        float size = Settings.GetSettingAsFloat("ar.marker.size", 0.4f);
        StartCoroutine(AddImage(texture2D, m_poi.title, size));
    }

    private IEnumerator AddImage(Texture2D imageToAdd, string title, float size)
    {
        yield return null;
        IReferenceImageLibrary library = _aRTrackedImageManager.referenceLibrary;

        if (library is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            mutableLibrary.ScheduleAddImageWithValidationJob(
            imageToAdd,
            title,
            size == 0 ? null : size /* in meters */);
        }
        m_markerImageHasBeenDownloaded = true;

#if UNITY_EDITOR
        OnMarkerDetected();
#endif
    }

    private void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (!m_markerImageHasBeenDownloaded)
        {
            return;
        }

        foreach (var image in eventArgs.added)
        {
            TryStartMarkerStabilization(image);
        }

        foreach (var image in eventArgs.updated)
        {
            TryStartMarkerStabilization(image);
        }
    }

    private void TryStartMarkerStabilization(ARTrackedImage trackedImage)
    {
        // Accept Tracking OR Limited (anything localized). ARKit reports detected static
        // images as Limited rather than Tracking, so requiring == Tracking stalls on iOS.
        if (trackedImage.trackingState == TrackingState.None)
        {
            return;
        }

        if (m_markerStabilizationCoroutine != null)
        {
            return;
        }

        m_candidateTrackedImage = trackedImage;
        m_markerStabilizationCoroutine = StartCoroutine(WaitForStableMarkerPose());
    }

    private IEnumerator WaitForStableMarkerPose()
    {
        int trackedFrames = 0;

        while (!m_markerHasBeenDetected && m_candidateTrackedImage != null)
        {
            if (m_candidateTrackedImage.trackingState != TrackingState.None)
            {
                trackedFrames++;
            }
            else
            {
                trackedFrames = 0;
            }

            if (trackedFrames >= _requiredTrackedFramesBeforePlacement)
            {
                m_detectedImageTransform = m_candidateTrackedImage.transform;
                OnMarkerDetected();
                yield break;
            }

            yield return null;
        }

        m_markerStabilizationCoroutine = null;
    }

    private void OnMarkerDetected()
    {
        _objectRoot.SetParent(m_detectedImageTransform, false);
        _objectRoot.position += m_heightOffset * Vector3.up;
        _objectRoot.localEulerAngles = (90 - m_angleToNorth) * Vector3.up;

        _objectRoot.gameObject.SetActive(true);
        m_markerHasBeenDetected = true;
        MarkerDetected?.Invoke();
        ToggleUI(false);
        SpawnGrassPatches();
    }

    private void SpawnGrassPatches()
    {
        GrassPatch grassPatch = null;

        foreach (Poi grassPatchPoi in Initializer.GetPoiChildren(m_grassPatchesPoi))
        {
            int randomIndex = Random.Range(0, _grassPatchPrefabs.Count);
            grassPatch = Instantiate(_grassPatchPrefabs[randomIndex], _objectRoot);
            m_grassPatches.Add(grassPatch);

            grassPatch.transform.localPosition = GetObjectPosition(grassPatchPoi.pid);
            grassPatch.transform.localEulerAngles = GetObjectRotation(grassPatchPoi) * Vector3.up;
            grassPatch.transform.localScale = GetObjectScale(grassPatchPoi);
        }
    }

    private Vector3 GetObjectPosition(string pid)
    {
        PoiLocation objectLocation = PoiLocationStore.GetPoiLocationById(pid);
        if (objectLocation == null)
        {
            Debug.LogError("No location for POI " + pid);
            return Vector3.zero;
        }

        ARLocation.DVector2 fromCenterToObject = ARLocation.Location.HorizontalVectorFromTo(m_poiARLocation, new ARLocation.Location(objectLocation));
        return fromCenterToObject.ToVector3();
    }

    private float GetObjectRotation(Poi objectLocationPoi)
    {
        return StringUtils.GetStringAsFloat(objectLocationPoi.spatial);
    }

    private Vector3 GetObjectScale(Poi objectLocationPoi)
    {
        return StringUtils.GetStringAsFloat(objectLocationPoi.extent, 1) * Vector3.one;
    }

    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        _audioSource.clip = clip;
        _audioSource.Play();
    }
#endregion
#endregion
}
