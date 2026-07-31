using System.Collections;
using System.Collections.Generic;
using Unity.Samples.ScreenReader;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.Events;
using UnityEngine.UI;

public class PoiList : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private PoiListItem _poiItemPrefab;
    [SerializeField] private SecretPoiListItem _secretPoiItemPrefab;
    [SerializeField] private ScrollRect _scrollRect;
    #endregion

    #region Private
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    private void OnEnable()
    {
        StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_contentRoot.gameObject));
    }
    #endregion

    #region Public
    public void Inflate(List<Wezit.Poi> pois, TourProgressionData tourProgressionData, MonoBehaviour monoBehaviour)
    {
        ResetContent();

        foreach (Wezit.Poi poi in pois)
        {
            if (poi.tags.Contains(Tags.SECRET_POI))
            {
                if (tourProgressionData.PercentOfCompletion < 1 && !PlayerManager.CurrentState.IsInDevMode)
                {
                    Debug.Log("User has not unlocked secret POI yet");
                    continue;
                }

                SecretPoiListItem secretPoiListItem = Instantiate(_secretPoiItemPrefab, _contentRoot);
                secretPoiListItem.Inflate(poi, tourProgressionData.GetPoiProgression(poi.pid), monoBehaviour, true);
                secretPoiListItem.NavigationButtonClicked.AddListener(OnPoiNavigationClicked);
                continue;
            }

            PoiListItem poiListItem = Instantiate(_poiItemPrefab, _contentRoot);

            poiListItem.Inflate(poi, tourProgressionData.GetPoiProgression(poi.pid), monoBehaviour, true);
            poiListItem.NavigationButtonClicked.AddListener(OnPoiNavigationClicked);
        }

        monoBehaviour.StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_contentRoot.gameObject));

        AssistiveSupport.notificationDispatcher.SendScreenChanged();
        this.DelayRefreshHierarchy();
    }

    public void ResetContent()
    {
        _scrollRect.content.localPosition = Vector3.zero;

        foreach (Transform child in _contentRoot)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region Private
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
    #endregion
    #endregion
}
