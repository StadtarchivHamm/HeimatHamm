using UnityEngine;
using TMPro;

public class POIDistanceWidget : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private TextMeshProUGUI _distanceText;
    [SerializeField] private GameObject _root;
    #endregion

    #region Private
    private Wezit.PoiLocation m_poiLocation;
    private Vector2 m_geoposition = Vector2.zero;
    private MonoBehaviour m_activeMonobehavour;
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    private void OnDisable()
    {
        Utils.MapUtils.UserLocationUpdated.RemoveListener(OnLocationUpdated);
    }

    private void OnDestroy()
    {
        Utils.MapUtils.UserLocationUpdated.RemoveListener(OnLocationUpdated);
    }
    #endregion
    #region Public
    public void Inflate(Wezit.PoiLocation poiLocation, MonoBehaviour activeMonobehavour = null)
    {
        m_activeMonobehavour = activeMonobehavour;

        _root.SetActive(PlayerManager.CurrentState.LastKnownPosition != Vector2.zero);
        m_poiLocation = poiLocation;
        if (m_poiLocation != null)
        {
            m_geoposition = new Vector2(m_poiLocation.lng, m_poiLocation.lat);
        }

        Utils.MapUtils.UserLocationUpdated.RemoveListener(OnLocationUpdated);
        Utils.MapUtils.UserLocationUpdated.AddListener(OnLocationUpdated);

        if (PlayerManager.CurrentState.IsGPSOn && PlayerManager.CurrentState.LastKnownPosition != Vector2.zero)
        {
            OnLocationUpdated(PlayerManager.CurrentState.LastKnownPosition);
        }
    }
    #endregion
    #region Private
    private void OnLocationUpdated(Vector2 userLocation)
    {
        if(!_root.activeInHierarchy)
        {
            _root.SetActive(true);
        }
        float distance = Utils.MapUtils.CalculateDistance(userLocation, m_geoposition);
        float distancekm = 0;
        float distancem = Mathf.Floor(distance);

        if(distance >= 1000)
        {
            distancekm = Mathf.Floor(distance / 1000);
            distancem = Mathf.Floor(distance - distancekm * 1000);
        }

        _distanceText.text = distance >= 1000 ? distancekm + "," + (int)distancem/100 + "km" : distancem + "m";

        if(m_activeMonobehavour != null)
        {
            m_activeMonobehavour.StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_root));
        }
        else
        {
            StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_root));
        }
    }
    #endregion
    #endregion
}
