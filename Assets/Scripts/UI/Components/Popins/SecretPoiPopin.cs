using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SecretPoiPopin : MonoBehaviour
{
    [SerializeField] private SecretPoiListItem _poiItem;
    [SerializeField] private Button _continue;

    public UnityEvent SecretPopinClicked = new UnityEvent();
    public void Init(MonoBehaviour monoBehaviour)
    {
        _poiItem.Inflate(PlayerManager.CurrentState.CurrentTour.children.Find(x => x.tags.Contains(Tags.SECRET_POI)), monoBehaviour);

        _poiItem.NavigationButtonClicked.RemoveAllListeners();
        _poiItem.NavigationButtonClicked.AddListener(OnPoiNavigationClicked);

        _poiItem.SecretPoiListItemClicked.RemoveAllListeners();
        _poiItem.SecretPoiListItemClicked.AddListener(OnItemClicked);

        _continue.onClick.RemoveAllListeners();
        _continue.onClick.AddListener(OnContinue);

        TogglePopin(true);
    }

    public void TogglePopin(bool isOn)
    {
        gameObject.SetActive(isOn);
    }

    private void OnPoiNavigationClicked(Wezit.Poi poi)
    {
        Wezit.PoiLocation poiLocation = PoiLocationStore.GetPoiLocationById(poi.pid);

        if (poiLocation == null)
        {
            return;
        }

        PlayerManager.CurrentState.NavigationGoalPOIPosition = new Vector2(poiLocation.lng, poiLocation.lat);
        PlayerManager.CurrentState.NavigationIsOn = true;
        AppManager.Instance.GoToState(KioskState.MAP);
    }

    private void OnContinue()
    {
        gameObject.SetActive(false);
    }

    private void OnItemClicked()
    {
        SecretPopinClicked?.Invoke();
    }
}
