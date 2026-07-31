using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;
using UniRx;
using System.Collections;
using UnityEngine.Android;
using UniRx.Async;
using InfinityCode.OnlineMapsExamples;
using Unity.Hierarchy;

public class TourMapView : BaseView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private ViewSelector _viewSelector;
    [SerializeField] private TutorialPopin _tutorialPopin;
    [Space]
	[SerializeField] private OnlineMaps _map;
	[SerializeField] private OpenRouteServiceExample _navigationService;
	[SerializeField] private TourMapPin _tourMapPinPrefab;
	[SerializeField] private MapListHorizontal _mapListHorizontal;
	[SerializeField] private GameObject _userMarkerPrefab;
	[Space]
	[SerializeField] private ARNotification _arNotification;
	[SerializeField] private SecretPoiNotification _secretPoiNotification;
	[SerializeField] private Toggle _wheelchairToggle;
	[SerializeField] private Button _centerOnUser;
	[SerializeField] private Toggle _rotateToNorthToggle;
	#endregion Serialize Fields

	#region Private m_Variables
	private Wezit.Tour m_tourData;
	private OnlineMapsCameraOrbit m_cameraOrbit;
	private OnlineMapsMarker3DManager m_mapsMarker3DManager;

	private OnlineMapsMarker3D m_userMarker;
	private List<GameObject> m_pins = new List<GameObject>();
	private Dictionary<Wezit.PoiLocation, Wezit.Poi> m_poisAndLocations = new Dictionary<Wezit.PoiLocation, Wezit.Poi>();

	// Dictionary to quickly get the pin corresponding to the selected Poi
	private Dictionary<Wezit.Poi, TourMapPin> m_poisAndPins = new Dictionary<Wezit.Poi, TourMapPin>();
	private TourMapPin m_previousHighlightedPin;
	private TourMapPin m_currentHighlightedPin;

	private Wezit.Poi m_lastPoiInRange;

	private string m_mapProviderSettingKey = "map.provider.url";
	private string m_mapProviderUrl;
	private float m_minDistanceToCenter;
	private Vector2 m_lastMapCenter;

	private Coroutine m_translationCoroutine;
	private Coroutine m_zoomCoroutine;
	private Coroutine m_rotationCoroutine;
	private Coroutine m_rotationToNorthCoroutine;
	private Coroutine m_userCompassCoroutine;

	private bool m_rotateToNorth;
	private bool m_useWheelchairMode;
	#endregion Private m_Variables
	#endregion Fields

	#region Properties
	#endregion Properties

	#region Methods
	#region Public
	public override void PrepareHideView()
	{
		base.PrepareHideView();

		MapUtils.StopLocationService();
		MapUtils.KeepRotating = false;
	}
	#endregion Public

	#region Private
	protected override async void InitViewContentByLang(Language language)
	{
		ViewManager.Instance.HideSpecificView(KioskState.DOWNLOAD);

		base.InitViewContentByLang(language);

		if (ViewManager.Instance.PreviousKioskState == KioskState.TOUR_INTRO ||
            ViewManager.Instance.PreviousKioskState == KioskState.DOWNLOAD ||
			ViewManager.Instance.PreviousKioskState == KioskState.POI_DETAILS)
		{
            _navigationService.RemoveRoutes();
        }

		if (PlayerManager.CurrentState.IsAudioDescription)
		{
			MenuManager.Instance.KioskStateHistory.Pop();
			SetState(KioskState.LIST);
		}

		_viewSelector.Init();

		m_tourData = PlayerManager.CurrentState.CurrentTour;

#if !UNITY_EDITOR
		PlayerManager.CurrentState.IsGPSOn = Input.location.isEnabledByUser;
#endif
		m_useWheelchairMode = PlayerManager.Player.UseWheelchairMode;
		_wheelchairToggle.SetIsOnWithoutNotify(m_useWheelchairMode);

		m_mapsMarker3DManager = _map.GetComponent<OnlineMapsMarker3DManager>();

		List<Vector2> poiCoordinates = new List<Vector2>();
		List<Wezit.Poi> locatedPois = new List<Wezit.Poi>();

		m_cameraOrbit = _map.GetComponent<OnlineMapsCameraOrbit>();
		m_cameraOrbit.OnCameraControl = OnMapRotated;
		m_cameraOrbit.OnChangedByInput = StopMovementCoroutines;

		OnlineMapsTileSetControl tileSetControl = _map.GetComponent<OnlineMapsTileSetControl>();
		tileSetControl.OnMapDrag -= StopMovementCoroutines;
		tileSetControl.OnMapDrag += StopMovementCoroutines;
		tileSetControl.OnMapZoom -= StopMovementCoroutines;
		tileSetControl.OnMapZoom += StopMovementCoroutines;

		// Start geolocating
		MapUtils.StartLocationService(this);
		MapUtils.StartRotationService(this);

		// Instantiate map markers and look for a map
		Wezit.PoiLocation poiLocation = null;
		string mapSource = "";

        PlayerManager.CurrentState.CurrentTourPoisLongLat.Clear();
        Wezit.Poi stationLocationPoi;

		foreach(Wezit.Poi poi in m_tourData.children)
        {
			if (poi.tags.Contains(Tags.SECRET_POI))
			{
				if (PlayerManager.Player.GetCurrentTourProgression().PercentOfCompletion < 1 && !PlayerManager.CurrentState.IsInDevMode)
				{
					continue;
				}
			}

			if (poi.children == null || poi.children.Count == 0)
            {
                Debug.LogWarning("No children for POI: " + poi.pid + " ; " + poi.title);
                continue;
			}

			stationLocationPoi = poi.children.Find(x => x.tags.Contains(Tags.POI_LOCATION));
            if (stationLocationPoi == null)
            {
                Debug.LogWarning("No location POI for POI: " + poi.pid + " ; " + poi.title);
                continue;
            }

            poiLocation = PoiLocationStore.GetPoiLocationById(stationLocationPoi.pid);
			if (poiLocation == null)
			{
				Debug.LogWarning("No location POI location for POI: " + poi.pid + " ; " + poi.title);
				continue;
			}

			Vector2 longlat = new Vector2(poiLocation.lng, poiLocation.lat);

            PlayerManager.CurrentState.CurrentTourPoisLongLat.Add(new TourPoiLongLat(poi, longlat));
			poiCoordinates.Add(longlat);

            locatedPois.Add(poi);
            m_poisAndLocations.Add(poiLocation, poi);

			if(string.IsNullOrEmpty(mapSource))
            {
				mapSource = poiLocation.GetMapSourceByTransformation(WezitSourceTransformation.tilesZip).Replace("metadata.json", "");
            }

			OnlineMapsMarker3D marker3D = m_mapsMarker3DManager.Create(poiLocation.lng, poiLocation.lat, _tourMapPinPrefab.gameObject);
			TourMapPin tourMapPinInstance = marker3D.instance.GetComponent<TourMapPin>();
			m_pins.Add(marker3D.instance);
			tourMapPinInstance.Inflate(poi);
			tourMapPinInstance.TourMapPinClicked.AddListener(OnMapPinClicked);
			m_poisAndPins.Add(poi, tourMapPinInstance);
		}

		if (PlayerManager.CurrentState.IsGPSOn)
        {
			m_userMarker = m_mapsMarker3DManager.Create(PlayerManager.CurrentState.LastKnownPosition.x, PlayerManager.CurrentState.LastKnownPosition.y, _userMarkerPrefab);
			m_userMarker.scale = 100f;

			if (m_userCompassCoroutine != null)
			{
				StopCoroutine(m_userCompassCoroutine);
			}
			m_userCompassCoroutine = StartCoroutine(RotateUserMarker());
		}

		m_minDistanceToCenter = Wezit.Settings.GetSettingAsFloat("map.settings.location.mindistance.value");

		await DisplayMap(mapSource);

        TourProgressionData tourProgressionData = PlayerManager.Player.GetCurrentTourProgression();
		_mapListHorizontal.Inflate(locatedPois, tourProgressionData, this);

		Wezit.Poi currentPoi = null;
		if(PlayerManager.CurrentState.CurrentPoi == null)
        {
			currentPoi = locatedPois[0];
        }
		else
        {
			currentPoi = PlayerManager.CurrentState.CurrentPoi;
		}

		Wezit.Poi currentPoiStationLocation = currentPoi.children.Find(x => x.tags.Contains(Tags.POI_LOCATION));

		if (currentPoiStationLocation != null)
		{
            Wezit.PoiLocation currentPoiLocation = PoiLocationStore.GetPoiLocationById(currentPoiStationLocation.pid);
			OnItemSelected(new Vector2(currentPoiLocation.lng, currentPoiLocation.lat), currentPoi);

            _mapListHorizontal.SelectPoi(currentPoi.pid);
        }

		if (PlayerManager.CurrentState.LastKnownPosition != Vector2.zero)
        {
			OnLocationChanged(PlayerManager.CurrentState.LastKnownPosition);

			CenterOnUser();
		}

		if (PlayerManager.CurrentState.NavigationIsOn && PlayerManager.CurrentState.IsGPSOn)
		{
			PlayerManager.CurrentState.NavigationIsOn = false;

            _navigationService.ComputeRoute(PlayerManager.CurrentState.LastKnownPosition, PlayerManager.CurrentState.NavigationGoalPOIPosition, m_useWheelchairMode);
        }

        _tutorialPopin.TogglePopin(!PlayerManager.Player.HasSeenTutorial);
    }

	protected override void ResetViewContent()
	{
		base.ResetViewContent();

		ResetMapRotation();

		_mapListHorizontal.ResetContent();

        foreach (GameObject mapPin in m_pins)
		{
			if (mapPin != null) Destroy(mapPin);
		}
		OnlineMapsMarker3DManager onlineMapsMarker3Ds = _map.GetComponent<OnlineMapsMarker3DManager>();
		onlineMapsMarker3Ds.RemoveAll();
		m_pins.Clear();
		m_poisAndLocations.Clear();
		m_poisAndPins.Clear();

		if(ViewManager.Instance.PreviousKioskState != KioskState.POI_DETAILS)
        {
			m_lastPoiInRange = null;
        }

		_arNotification.Close(false);
        _secretPoiNotification.Close(false);
    }

	protected override void AddListeners()
	{
		base.AddListeners();

		_mapListHorizontal.PoiSelected.AddListener(OnItemSelected);
		_mapListHorizontal.PoiNavigationClicked.AddListener(OnNavigationButtonClicked);

		MapUtils.UserLocationUpdated.AddListener(OnLocationChanged);

		_wheelchairToggle.onValueChanged.AddListener(OnWheelchairToggled);
		_centerOnUser.onClick.AddListener(CenterOnUser);
		_rotateToNorthToggle.onValueChanged.AddListener(OnRotateToNorthToggled);
    }

    protected override void RemoveListeners()
	{
		base.RemoveListeners();

		_mapListHorizontal.PoiSelected.RemoveAllListeners();
        _mapListHorizontal.PoiNavigationClicked.RemoveListener(OnNavigationButtonClicked);

        MapUtils.UserLocationUpdated.RemoveListener(OnLocationChanged);

        _wheelchairToggle.onValueChanged.RemoveListener(OnWheelchairToggled);
        _centerOnUser.onClick.RemoveListener(CenterOnUser);
		_rotateToNorthToggle.onValueChanged.RemoveAllListeners();
    }

	private void OnLocationChanged(Vector2 location)
	{
        if (MapUtils.CalculateDistance(location, m_lastMapCenter) > m_minDistanceToCenter)
        {
			CenterOnUser();
			m_lastMapCenter = location;
        }


		if (m_userMarker == null)
        {
			m_userMarker = m_mapsMarker3DManager.Create(PlayerManager.CurrentState.LastKnownPosition.x, PlayerManager.CurrentState.LastKnownPosition.y, _userMarkerPrefab);
			m_userMarker.scale = 100f;
			if (m_userCompassCoroutine != null)
			{
				StopCoroutine(m_userCompassCoroutine);
			}
			m_userCompassCoroutine = StartCoroutine(RotateUserMarker());
		}
		m_userMarker.position = location;

		Wezit.Poi poiInRange = CheckPoisInRange(location);

		if (m_lastPoiInRange != poiInRange && poiInRange != null)
		{
			if (poiInRange.tags.Contains(Tags.SECRET_POI))
			{
				if (PlayerManager.Player.GetCurrentTourProgression().PercentOfCompletion < 1)
				{
					return;
				}

                m_lastPoiInRange = poiInRange;
                PlayerManager.CurrentState.LastPOIInRange = poiInRange;


                if (PlayerManager.Player.IsARCompatible)
                {
					_secretPoiNotification.Inflate(true, poiInRange);
                }

                _mapListHorizontal.SelectPoi(poiInRange.pid);
				return;
			}

			m_lastPoiInRange = poiInRange;
			PlayerManager.CurrentState.LastPOIInRange = poiInRange;

			if (PlayerManager.Player.IsARCompatible)
			{
				_arNotification.Inflate(true, poiInRange);
			}

            _mapListHorizontal.SelectPoi(poiInRange.pid);
		}
	}

	// Map management
	private async UniTask DisplayMap(string mapSource)
    {
        // Display map
        OnlineMapsLimits limits = _map.GetComponent<OnlineMapsLimits>();

        if (!string.IsNullOrEmpty(mapSource))
		{
			string mapMetadataJsonString = await FileUtils.RequestTextContent(mapSource + "/metadata.json", 5);
			Wezit.MapMetadata mapMetadata = JsonUtility.FromJson<Wezit.MapMetadata>(mapMetadataJsonString);
			Vector4 bounds = mapMetadata.GetBounds();

			// Add limits to the map
			limits.minLongitude = bounds.x;
			limits.minLatitude = bounds.y;

			limits.maxLongitude = bounds.z;
			limits.maxLatitude = bounds.w;

			limits.minZoom = mapMetadata.minzoom;
			limits.maxZoom = mapMetadata.maxzoom;

			limits.positionRangeType = OnlineMapsPositionRangeType.center;

			_map.customProviderURL = mapSource + "/{zoom}/{x}/{y}.jpg";
		}
		else
		{
			limits.minLongitude = -180;
			limits.minLatitude = -90;

			limits.maxLongitude = 180;
			limits.maxLatitude = 90;

			limits.minZoom = 1;
			limits.maxZoom = 21;

			m_mapProviderUrl = Wezit.Settings.GetSettingAsCleanedText(m_mapProviderSettingKey);
			if (!string.IsNullOrEmpty(m_mapProviderUrl))
			{
				_map.customProviderURL = m_mapProviderUrl;
			}
			else
			{
				_map.customProviderURL = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
			}
        }

        limits.usePositionRange = true;
        limits.useZoomRange = true;
        limits.ApplySettings();

        // Change empty tile color
        ColorUtility.TryParseHtmlString(m_tourData.CleanedSubject, out _map.emptyColor);
		_map.emptyColor = new Color(32, 32, 89);
	}

	private Wezit.Poi CheckPoisInRange(Vector2 location)
    {
		Wezit.Poi result = null;
		foreach(Wezit.PoiLocation poiLocation in m_poisAndLocations.Keys)
        {
			if(IsInRange(location, poiLocation).isInRange)
            {
				result = m_poisAndLocations[poiLocation];
            }
        }

		return result;
    }

	private (bool isInRange, float distance) IsInRange(Vector2 location, Wezit.PoiLocation poiLocation)
    {
		float distance = MapUtils.CalculateDistance(location, new Vector2(poiLocation.lng, poiLocation.lat));
		return (distance < (poiLocation.radius == 0 ? 15 : poiLocation.radius), distance);
    }

	private void CenterOnUser()
    {
		if (PlayerManager.CurrentState.LastKnownPosition != Vector2.zero)
		{
			if (m_translationCoroutine != null)
			{
				StopCoroutine(m_translationCoroutine);
			}

			m_translationCoroutine = StartCoroutine(TranslateMapSmoothlyToPoint(new Vector2(PlayerManager.CurrentState.LastKnownPosition.x, PlayerManager.CurrentState.LastKnownPosition.y), 20));
        }
		else
        {
#if UNITY_ANDROID
			if(!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
				Permission.RequestUserPermission(Permission.FineLocation);
            }
#elif UNITY_IOS
			Input.location.Start();
			Input.location.Stop();
#endif
			if (m_translationCoroutine != null)
			{
				StopCoroutine(m_translationCoroutine);
			}
		}
	}

	private void OnMapRotated()
    {
        foreach (KeyValuePair<Wezit.Poi, TourMapPin> mapPin in m_poisAndPins)
        {
			mapPin.Value.Rotate(m_cameraOrbit.rotation.y);
        }
    }

	private void ResetMapRotation()
	{
		if (m_rotationCoroutine != null)
		{
			StopCoroutine(m_rotationCoroutine);
        }

        if (m_rotationToNorthCoroutine != null)
        {
            StopCoroutine(m_rotationToNorthCoroutine);
            m_rotationToNorthCoroutine = null;
        }

        m_rotationCoroutine = StartCoroutine(RotateMapToAngle(Vector2.zero));
    }

	private void OnMapPinClicked(Wezit.Poi poi)
	{
		if (poi == null)
        {
			return;
        }

		if (m_currentHighlightedPin.Poi == poi)
        {
            PlayerManager.CurrentState.CurrentPoi = poi;
            PlayerManager.CurrentState.CurrentStationLocationPoi = poi.children.Find(x => x.tags.Contains(Tags.POI_LOCATION));
            AppManager.Instance.GoToState(poi.tags.Contains(Tags.SECRET_POI) ? KioskState.SECRET_POI : KioskState.POI_DETAILS);

        }
		else 
		{ 
			_mapListHorizontal.SelectPoi(poi.pid);
		}
    }

	private void OnItemSelected(Vector2 geolocation, Wezit.Poi poi)
	{
        if (m_translationCoroutine != null)
        {
			StopCoroutine(m_translationCoroutine);
        }

		float zoom = 16.5f;

		m_translationCoroutine = StartCoroutine(TranslateMapSmoothlyToPoint(geolocation, zoom));

		TourMapPin selectedPin = m_poisAndPins[poi];
		if (m_currentHighlightedPin != null && m_currentHighlightedPin != selectedPin)
		{
			m_previousHighlightedPin = m_currentHighlightedPin;
			m_previousHighlightedPin.Highlight(false);
		}
		m_currentHighlightedPin = selectedPin;
		m_currentHighlightedPin.Highlight(true);
	}

	private IEnumerator TranslateMapSmoothlyToPoint(Vector2 geolocation, float zoom)
    {
		float timer = 0;
		float goalX = geolocation.x;
		float goalY = geolocation.y;

		if (m_zoomCoroutine != null)
		{
			StopCoroutine(m_zoomCoroutine);
		}
        m_zoomCoroutine = StartCoroutine(ZoomMapSmoothly(zoom));

		while (_map.position != geolocation && timer < 5f)
		{
			timer += Time.deltaTime;
			_map.SetPosition(Mathf.Lerp(_map.position.x, goalX, Time.deltaTime * 2f), Mathf.Lerp(_map.position.y, goalY, Time.deltaTime * 2f));
			yield return null;
		}
		_map.SetPositionAndZoom(geolocation.x, geolocation.y, zoom);
	}

	private IEnumerator ZoomMapSmoothly(float newZoom)
    {
		float timer = 0;

		while (_map.floatZoom != newZoom && timer < 2f)
		{
			timer += Time.deltaTime;
			_map.floatZoom = Mathf.Lerp(_map.floatZoom, newZoom, Time.deltaTime * 2f);
			yield return null;
		}
		_map.floatZoom = newZoom;
	}

	private IEnumerator RotateMapToAngle(Vector2 goal, float duration = 5f)
	{
        if (m_cameraOrbit == null)
        {
			yield break;
        }

		float timer = 0;

		while (m_cameraOrbit.rotation != goal && timer < duration)
		{
			timer += Time.deltaTime;
			m_cameraOrbit.rotation = Vector2.Lerp(m_cameraOrbit.rotation, goal, Time.deltaTime * 2f);
			yield return null;
		}

		m_cameraOrbit.rotation = goal;
	}

	private void StopMovementCoroutines()
	{
		if (m_translationCoroutine != null)
		{
			StopCoroutine(m_translationCoroutine);
		}

		if (m_zoomCoroutine != null)
		{
			StopCoroutine(m_zoomCoroutine);
		}

		if (m_rotationCoroutine != null)
		{
			StopCoroutine(m_rotationCoroutine);
		}
	}

	private IEnumerator RotateUserMarker()
    {
        while (true)
        {
			yield return null;
			if(m_userMarker != null)
            {
				m_userMarker.rotationY = -MapUtils.RotationToNorth;
            }
        }
    }

	private void OnNavigationButtonClicked(Wezit.Poi poi)
	{
		if (!PlayerManager.CurrentState.IsGPSOn)
		{
			OnMapPinClicked(poi);
			return;
        }

        Wezit.PoiLocation poiLocation = PoiLocationStore.GetPoiLocationById(poi.pid);

        if (poiLocation == null)
        {
            Debug.LogWarning("No poi location for POI " + poi.pid);
            return;
        }

        PlayerManager.CurrentState.NavigationGoalPOIPosition = new Vector2(poiLocation.lng, poiLocation.lat);

        _navigationService.ComputeRoute(PlayerManager.CurrentState.LastKnownPosition, PlayerManager.CurrentState.NavigationGoalPOIPosition, m_useWheelchairMode);
		OnRotateToNorthToggled(true);
	}

	private void OnWheelchairToggled(bool isWheelchair)
	{
		PlayerManager.Player.UseWheelchairMode = m_useWheelchairMode = isWheelchair;
		PlayerManager.Player.Save();
    }

	private void OnRotateToNorthToggled(bool isRotateToNorth)
	{
		m_rotateToNorth = isRotateToNorth;

		if (isRotateToNorth)
		{
			if (m_rotationCoroutine != null)
			{
				StopCoroutine(m_rotationCoroutine);
			}

			if (m_rotationToNorthCoroutine != null)
			{
				return;
			}
			else
			{
				m_rotationToNorthCoroutine = StartCoroutine(RotateMapToNorth());
			}
		}
		else
        {
            if (m_rotationToNorthCoroutine != null)
            {
                StopCoroutine(m_rotationToNorthCoroutine);
				m_rotationToNorthCoroutine = null;
            }
			ResetMapRotation();
        }
    }

    private IEnumerator RotateMapToNorth()
    {
        yield return StartCoroutine(RotateMapToAngle(-MapUtils.RotationToNorth * Vector2.up, 2f));
        while (m_rotateToNorth)
        {
            m_cameraOrbit.rotation = -MapUtils.RotationToNorth * Vector2.up;
            yield return null;
        }
        m_rotationToNorthCoroutine = null;
    }
    #endregion Private
    #endregion Methods
}