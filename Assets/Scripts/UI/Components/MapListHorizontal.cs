using DanielLochner.Assets.SimpleScrollSnap;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wezit;
using static OnlineMapsAMapSearchResult;

public class MapListHorizontal : MonoBehaviour
{
    #region Fields
    #region SerializeField
    [SerializeField] private Transform _prefabRoot;
    [SerializeField] private PoiListItem _itemPrefab;
    [SerializeField] private SecretPoiListItem _secretPoiItemPrefab;
    [SerializeField] private SimpleScrollSnap _simpleScrollSnap;
    #endregion
    #region Private
    List<PoiListItem> m_Items = new List<PoiListItem>();
    #endregion
    #endregion

    #region Properties
    public UnityEvent<Poi> PoiNavigationClicked = new UnityEvent<Poi>();
    public UnityEvent<Vector2, Poi> PoiSelected = new UnityEvent<Vector2, Poi>();
    #endregion

    #region Methods
    #region Monobehaviour
    #endregion
    #region Public
    public void Inflate(List<Poi> pois, TourProgressionData tourProgressionData, MonoBehaviour activeMonobehaviour)
    {
        ResetContent();

        foreach (Poi poi in pois)
        {
            if (poi.tags.Contains(Tags.SECRET_POI))
            {
                if (PlayerManager.Player.GetCurrentTourProgression().PercentOfCompletion < 1 && !PlayerManager.CurrentState.IsInDevMode)
                {
                    Debug.Log("User has not unlocked secret POI yet");
                    continue;
                }

                SecretPoiListItem secretPoiListItem = Instantiate(_secretPoiItemPrefab, _prefabRoot);
                secretPoiListItem.Inflate(poi, tourProgressionData.GetPoiProgression(poi.pid), activeMonobehaviour, true);
                secretPoiListItem.NavigationButtonClicked.AddListener(OnItemNavigationClicked);
                m_Items.Add(secretPoiListItem);

                continue;
            }

            PoiListItem itemInstance = Instantiate(_itemPrefab, _prefabRoot);
            itemInstance.name = poi.CleanedTitle;
            itemInstance.Inflate(poi, tourProgressionData.GetPoiProgression(poi.pid), activeMonobehaviour);
            itemInstance.NavigationButtonClicked.AddListener(OnItemNavigationClicked);
            m_Items.Add(itemInstance);
        }

        _simpleScrollSnap.Setup();
        _simpleScrollSnap.OnPanelCentered.AddListener(OnItemSelected);
        _simpleScrollSnap.GetComponent<ScrollRect>().enabled = pois.Count > 1;
    }

    public void SelectPoi(string pid)
    {
        int index = (m_Items.FindIndex(0, m_Items.Count - 1, x => x.pid == pid) + m_Items.Count) % m_Items.Count;
        StartCoroutine(GoToPanelCoroutine(index));
    }

    private IEnumerator GoToPanelCoroutine(int index)
    {
        while (_simpleScrollSnap.SelectedPanel != index)
        {
            _simpleScrollSnap.GoToPanel(index);
            yield return null;
        }
    }

    public void ResetContent()
    {
        foreach(PoiListItem item in m_Items)
        {
            Destroy(item.gameObject);
        }
        m_Items.Clear();
        _simpleScrollSnap.Setup();
    }
    #endregion
    #region Private
    private void OnItemNavigationClicked(Poi poi)
    {
        PoiNavigationClicked?.Invoke(poi);
    }

    private void OnItemSelected(int centered, int selected)
    {
        PoiSelected?.Invoke(m_Items[centered].Geolocation, m_Items[centered].Poi);
    }
    #endregion
    #endregion
}
