using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Wezit;

public class ARManager : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Camera _arCamera;
    [SerializeField] private ARTrackedImageManager _aRTrackedImageManager;
    [SerializeField] private ARSession _arSession;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject _uiRoot;
    [SerializeField] private Transform _objectRoot;
    [Header("Tracking Stabilization")]
    [SerializeField, Min(1)] private int _requiredTrackedFramesBeforePlacement = 5;
    [Header("Occlusion management")]
    [SerializeField] private ARPlaneManager _planeManager;
    [SerializeField] private GameObject _shadowCasterARPlane;
    [SerializeField] private GameObject _occlusionARPlane;
    [Space]
    [Header("Avatar")]
    [SerializeField] private AvatarManager _avatarManager;
    [SerializeField] private AudioClip _avatarAppearAudioClip;
    [Header("Hidden Object")]
    [SerializeField] private Transform _hiddenObjectRoot;
    [SerializeField] private AudioClip _hiddenObjectAppearClip;
    [SerializeField] private AudioClip _hiddenObjectFoundAudioClip;
    [Header("Portal")]
    [SerializeField] private TimePortal _timePortal;
    [SerializeField] private AudioClip _portalTraversedAudioClip;
    [SerializeField] private AudioClip _portalAppearAudioClip;
    [Header("Past Objects")]
    [SerializeField] private ARVideoPlayer _pastVideo;
    [SerializeField] private Transform _pastObjectsRoot;
    [SerializeField] private ARImageSupport _smallImageSupport;
    [SerializeField] private ARImageSupport _mediumImageSupport;
    [SerializeField] private ARImageSupport _largeImageSupport;
    [Header("Testing")]
    [SerializeField] private TextMeshProUGUI _testAngleText;
    [SerializeField] private GameObject _qrCode;
    #endregion

    #region Private
    private string m_useOcclusionPlanesSettingKey = "ar.use.occlusion.toggle";
    private string m_avatarLookAtUserSettingKey = "ar.avatar.lookatuser.bool";
    private string m_pastAudioStartDelaySettingKey = "ar.avatar.audio.start.delay.value";

    private Poi m_poi;
    private ARLocation.Location m_poiARLocation;
    private List<GameObject> m_pastObjects = new List<GameObject>();
    private float m_angleToNorth;
    private float m_heightOffset;

    private Poi m_stationLocationPoi;
    private Poi m_avatarPositionPoi;
    private Poi m_hiddenObjectPoi;
    private Poi m_portalPositionPoi;
    private Poi m_pastObjectsPoi;


    private Coroutine m_markerStabilizationCoroutine;
    private ARTrackedImage m_candidateTrackedImage;
    private bool m_markerImageHasBeenDownloaded;
    private bool m_markerHasBeenDetected;
    private Transform m_detectedImageTransform;
    private bool m_portalHasBeenEntered;
    #endregion
    #endregion

    #region Properties
    public UnityEvent MarkerDetected = new UnityEvent();
    public UnityEvent HiddenObjectFound = new UnityEvent();
    public UnityEvent PastEntered = new UnityEvent();
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
        m_portalHasBeenEntered = false;
        m_markerImageHasBeenDownloaded = false;
    }

    private void Start()
    {
        AddListeners();

        MatomoAnalyticsManager.Instance.RecordARSessionStarted();
        MatomoAnalyticsManager.Instance.RecordARSessionStartedForTour(PlayerManager.CurrentState.CurrentTour.pid, PlayerManager.CurrentState.CurrentTour.CleanedTitle);
    }

    IEnumerator ResetARSession()
    {
        _arSession.Reset(); // Forces ARKit to fully reinitialize
        yield return new WaitUntil(() => ARSession.state == ARSessionState.SessionTracking);
    }

    private void LateUpdate()
    {
        if (_objectRoot != null)
        {
            _objectRoot.eulerAngles = _objectRoot.eulerAngles.y * Vector3.up;
            //_objectRoot.localPosition = m_heightOffset * Vector3.up;
        }
    }
    #endregion

    #region Public
    public void SetAngle(float angle)
    {
        _objectRoot.localEulerAngles = (90 - m_angleToNorth + angle) * Vector3.up;
        _testAngleText.text = angle.ToString() + ", " + _objectRoot.localEulerAngles.y;
    }

    public async void Inflate(Poi currentPoi, Poi stationLocationPoi, Poi avatarPositionPoi, Poi hiddenObjectPoi, Poi portalLocationPoi, Poi pastObjectsPoi)
    {
        _planeManager.planePrefab = Settings.GetSettingAsBool(m_useOcclusionPlanesSettingKey) ? _occlusionARPlane : _shadowCasterARPlane;

        m_poi = currentPoi;
        m_stationLocationPoi = stationLocationPoi;
        m_avatarPositionPoi = avatarPositionPoi;
        m_hiddenObjectPoi = hiddenObjectPoi;
        m_portalPositionPoi = portalLocationPoi;
        m_pastObjectsPoi = pastObjectsPoi;
        _objectRoot.gameObject.SetActive(false);
        _hiddenObjectRoot.gameObject.SetActive(false);

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

        if (m_markerHasBeenDetected)
        {
            OnMarkerDetected();
        }
    }

    public void ToggleUI(bool isOn)
    {
        _uiRoot.SetActive(isOn);
    }

    public void OnIntroFinished()
    {
        SpawnHiddenObject();
    }

    public void ShowPortal()
    {
        PlaySoundEffect(_portalAppearAudioClip);

        _timePortal.Appear();

        if (PlayerManager.CurrentState.IsInDevMode)
        {
            OnUserEnteredPast();
        }
    }

    public void StartAvatarAnimation(string animationTag)
    {
        _avatarManager.StartAvatarAnimation(animationTag);
    }

    public void ToggleAvatarTalk(bool talk)
    {
        _avatarManager.ToggleTalking(talk);
    }

    public void PauseVideo(bool isPaused)
    {
        _pastVideo.PauseVideo(isPaused);
    }
    #endregion

    #region Private
    private void AddListeners()
    {
        _timePortal.UserEnteredPast.RemoveAllListeners();
        _timePortal.UserEnteredPast.AddListener(OnUserEnteredPast);
    }

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
    }

    private void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Ignore detections until this session's reference image has been scheduled
        // into the library. Safe only because ARView.HideView() resets the library to
        // null each session (empty until AddImage commits the current marker). If that
        // reset is ever removed, a stale early 'added' event would be dropped here and
        // never recovered — ARFoundation re-fires 'updated', not 'added'.
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

        PlaySoundEffect(_avatarAppearAudioClip);

        _objectRoot.gameObject.SetActive(true);
        m_markerHasBeenDetected = true;
        MarkerDetected?.Invoke();
        PlaceAvatar();
        PlacePortal();
        ToggleUI(false);
    }

    private void PlaceAvatar()
    {
        _avatarManager.gameObject.SetActive(true);
        _avatarManager.SelectAvatar(PlayerManager.CurrentState.CurrentAvatarType);
        _avatarManager.PlaceAvatar(GetObjectPosition(m_avatarPositionPoi.pid),
                                   GetObjectRotation(m_avatarPositionPoi),
                                   GetObjectScale(m_avatarPositionPoi),
                                   _arCamera,
                                   Settings.GetSettingAsBool(m_avatarLookAtUserSettingKey));
    }

    private async void SpawnHiddenObject()
    {
        _hiddenObjectRoot.gameObject.SetActive(true);
        await m_hiddenObjectPoi.AreRelationsSet();

        if (m_hiddenObjectPoi.ThreeDRelations?.Count == 0)
        {
            Debug.LogError("No 3D object in POI " + m_hiddenObjectPoi.pid);
            return;
        }
        
        GameObject hiddenObject = await Utils.GLTFSpawner.SpawnGLTF(_hiddenObjectRoot, m_hiddenObjectPoi);

        if (hiddenObject == null)
        {
            Debug.LogError("Failed to spawn hidden object for POI: " + m_hiddenObjectPoi.pid);
            return;
        }

        _hiddenObjectRoot.localPosition = GetObjectPosition(m_hiddenObjectPoi.pid);
        _hiddenObjectRoot.localEulerAngles = GetObjectRotation(m_hiddenObjectPoi) * Vector3.up;
        _hiddenObjectRoot.localScale = GetObjectScale(m_hiddenObjectPoi);
        hiddenObject.AddComponent<HiddenObject>().Init().ObjectClicked.AddListener(OnHiddenObjectClicked);
        hiddenObject.tag = "HiddenObject";
    }

    private void OnHiddenObjectClicked(HiddenObject hiddenObject)
    {
        HiddenObjectFound?.Invoke();
        _hiddenObjectRoot.gameObject.SetActive(false);
        SpawnPastObjects();

        PlaySoundEffect(_hiddenObjectFoundAudioClip);
    }

    private void PlacePortal()
    {
        _timePortal.Hide();
        if (m_portalPositionPoi == null)
        {
            Debug.LogError("No portal position POI, portal will remain hidden");
            return;
        }
        _timePortal.PlacePortal(GetObjectPosition(m_portalPositionPoi.pid), GetObjectRotation(m_portalPositionPoi));
    }

    private void OnUserEnteredPast()
    {
        if (!m_portalHasBeenEntered)
        {
            _avatarManager.gameObject.SetActive(false);

            PlaySoundEffect(_portalTraversedAudioClip);

            foreach (GameObject pastObject in m_pastObjects)
            {
                pastObject.SetLayerRecursively(0);
            }

            StartCoroutine(WaitBeforeStartingVideo());

            PastEntered?.Invoke();
            m_portalHasBeenEntered = true;
        }
    }

    private IEnumerator WaitBeforeStartingVideo()
    {
        float delay = Settings.GetSettingAsFloat(m_pastAudioStartDelaySettingKey, 10);
        yield return new WaitForSeconds(delay);

        //_pastVideo.PlayVideo();
    }

    private async void SpawnPastObjects()
    {
        _pastVideo.Inflate(m_pastObjectsPoi, _arCamera);
        _pastVideo.gameObject.transform.localPosition = GetObjectPosition(m_pastObjectsPoi.pid);
        _pastVideo.gameObject.transform.localScale = GetObjectScale(m_pastObjectsPoi);
        _pastVideo.gameObject.SetLayerRecursively(6);
        m_pastObjects.Add(_pastVideo.gameObject);

        GameObject pastObject = null;

        foreach (Poi pastObjectPoi in Initializer.GetPoiChildren(m_pastObjectsPoi))
        {
            await pastObjectPoi.AreRelationsSet();

            if (pastObjectPoi.ThreeDRelations?.Count > 0)
            {
                pastObject = await Utils.GLTFSpawner.SpawnGLTF(_pastObjectsRoot, pastObjectPoi);
            }
            else
            {
                ARImageSupport aRImageSupport = Instantiate(pastObjectPoi.type.Contains("small") ? _smallImageSupport :
                                                            pastObjectPoi.type.Contains("medium") ? _mediumImageSupport :
                                                            pastObjectPoi.type.Contains("large") ? _largeImageSupport :
                                                            _smallImageSupport, _pastObjectsRoot);
                aRImageSupport.Inflate(pastObjectPoi);
                pastObject = aRImageSupport.gameObject;
            }

            m_pastObjects.Add(pastObject);
            pastObject.transform.localPosition = GetObjectPosition(pastObjectPoi.pid);
            pastObject.transform.localEulerAngles = GetObjectRotation(pastObjectPoi) * Vector3.up;
            pastObject.transform.localScale = GetObjectScale(pastObjectPoi);
            pastObject.SetLayerRecursively(6);
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
