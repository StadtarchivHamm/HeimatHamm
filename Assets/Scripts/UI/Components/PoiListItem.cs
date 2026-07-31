using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.Samples.ScreenReader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wezit;

public class PoiListItem : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] protected Button _button;
    [SerializeField] protected AccessibleButton _accessibleButton;
    [SerializeField] protected TextMeshProUGUI _title;
    [SerializeField] protected RawImage _poiThumbnail;
    [SerializeField] protected Button _navigationButton;
    [Space]
    [SerializeField] protected Button _itemButton;
    [SerializeField] protected RawImage _itemThumbnail;
    [SerializeField] protected GameObject _seedRoot;

    [SerializeField] protected POIDistanceWidget _distance;
    #endregion
    #region Private
    protected Poi m_poi;
    protected Vector2 m_geolocation;
    protected Poi m_stationLocationPoi;
    #endregion
    #endregion

    #region Properties
    public UnityEvent<Poi> NavigationButtonClicked = new UnityEvent<Poi>();

    public Vector2 Geolocation
    {
        get
        {
            return m_geolocation;
        }
    }

    public string pid
    {
        get
        {
            return m_poi == null ? "" : m_poi.pid;
        }
    }

    public Poi Poi
    {
        get
        {
            return m_poi;
        }
    }
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion

    #region Public
    public virtual PoiListItem Inflate(Poi poi, PoiProgressionData poiProgressionData, MonoBehaviour monoBehaviour, bool hasDistance = true, bool isInInventory = false)
    {
        m_poi = poi;

        _button.onClick.AddListener(OnButtonClicked);

        _navigationButton.gameObject.SetActive(PlayerManager.CurrentState.IsGPSOn && PlayerManager.CurrentState.IsUserInTheArea && !PlayerManager.CurrentState.IsAudioDescription);
        _navigationButton.onClick.AddListener(OnNavigationButtonClicked);

        if (_itemButton != null)
        {
            _itemButton.onClick.RemoveListener(OnItemButtonClicked);
            _itemButton.onClick.AddListener(OnItemButtonClicked);
            _itemButton.gameObject.SetActive((poiProgressionData.HasCollectedItem || poiProgressionData.HasCollectedSeed) 
                                              && !PlayerManager.CurrentState.IsAudioDescription);
            if (poiProgressionData.HasCollectedItem || poiProgressionData.HasCollectedSeed)
            {
                Utils.ImageUtils.LoadImage(_itemThumbnail, monoBehaviour, poi.children.Find(x => x.tags.Contains(Tags.HIDDEN_OBJECT)), fillParent:false);
            }
        }

        if (_seedRoot != null)
        {
            _seedRoot.SetActive(poiProgressionData.HasCollectedSeed && !PlayerManager.CurrentState.IsAudioDescription);
        }

        _title.text = poi.CleanedTitle;
        _accessibleButton.SetLabel(poi.CleanedTitle);
        _accessibleButton.value = poi.CleanedTitle;

        m_stationLocationPoi = poi.children.Find(x => x.tags.Contains(Tags.POI_LOCATION));
        PoiLocation poiLocation = PoiLocationStore.GetPoiLocationById(m_stationLocationPoi?.pid);
        if (poiLocation != null)
        {
            m_geolocation = new Vector2(poiLocation.lng, poiLocation.lat);
        }

        if (hasDistance)
        {
            _distance.Inflate(poiLocation, monoBehaviour);
        }

        Utils.ImageUtils.LoadImage(_poiThumbnail, monoBehaviour, m_stationLocationPoi);

        return this;
    }

    public void Inflate(Poi poi, MonoBehaviour monoBehaviour)
    {
        Inflate(poi, null, monoBehaviour);
    }
    #endregion
    #region Private
    protected virtual void OnButtonClicked()
    {
        PlayerManager.CurrentState.CurrentPoi = m_poi;
        PlayerManager.CurrentState.CurrentStationLocationPoi = m_stationLocationPoi;
        AppManager.Instance.GoToState(KioskState.POI_DETAILS);
    }

    protected void OnNavigationButtonClicked()
    {
        NavigationButtonClicked?.Invoke(m_stationLocationPoi);
    }

    protected void OnItemButtonClicked()
    {
        PlayerManager.CurrentState.CurrentHiddenObjectPoi = m_poi.children.Find(x => x.tags.Contains(Tags.HIDDEN_OBJECT));
        AppManager.Instance.GoToState(KioskState.HIDDEN_OBJECT);
    }
    #endregion
    #endregion
}
