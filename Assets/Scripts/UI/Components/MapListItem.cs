using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Wezit;
using TMPro;

public class MapListItem : MonoBehaviour
{
    #region Fields
    #region SerializeField
    [SerializeField] private Button _button;
    [SerializeField] private RawImage _image = null;
    [SerializeField] private LayoutElement _imageMask = null;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private POIDistanceWidget _distance;
    #endregion
    #region Private
    private Tour m_Tour;
    private Poi m_poi;
    private Vector2 m_geolocation;
    #endregion
    #endregion

    #region Properties
    public UnityEvent<Tour> ItemClickedTour = new UnityEvent<Tour>();
    public UnityEvent<Poi> ItemClickedPoi = new UnityEvent<Poi>();
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
            return m_Tour == null ? (m_poi == null ? "" : m_poi.pid) : m_Tour.pid;
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
    #region Monobehaviour
    #endregion

    #region Public

    public void Inflate(Poi poi, PoiLocation poiLocation, MonoBehaviour activeMonobehavour)
    {
        m_poi = poi;

        _title.text = m_poi.title;

        Utils.ImageUtils.LoadRefImage(_image, activeMonobehavour, m_poi);

        if (poiLocation != null)
        {
            m_geolocation = new Vector2(poiLocation.lng, poiLocation.lat);
        }
        _button.onClick.AddListener(OnButtonClickPoi);

        _distance.Inflate(poiLocation, activeMonobehavour);
    }
    #endregion
    #region Private

    private void OnButtonClickPoi()
    {
        ItemClickedPoi?.Invoke(m_poi);
    }
    #endregion
    #endregion
}
