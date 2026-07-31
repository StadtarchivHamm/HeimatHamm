using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Wezit;

public class TourMapPin : MonoBehaviour, IPointerClickHandler
{
    #region Fields
    #region SerializeField
    [SerializeField] private GameObject _highlightedRoot;
    [SerializeField] private SpriteRenderer _characterSpriteRenderer;
    [Space]
    [SerializeField] private GameObject _notHighlightedRoot;
    [SerializeField] private GameObject _completedRoot;
    [SerializeField] private GameObject _newRoot;
    [Space]
    [SerializeField] private Transform _pivot;
    #endregion
    #region Private
    private Poi m_poi;
    #endregion
    #endregion

    #region Properties
    public UnityEvent<Poi> TourMapPinClicked = new UnityEvent<Poi>();
    public Poi Poi {  get { return m_poi; } }
    #endregion

    #region Methods
    #region Monobehaviour
    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_poi != null)
        {
            TourMapPinClicked?.Invoke(m_poi);
        }
    }
    #endregion
    #region Public
    public void Inflate(Poi poi)
    {
        m_poi = poi;
        name = m_poi.title;

        bool completed = PlayerManager.Player.GetPoiProgression(poi.pid).HasCollectedSeed;

        _completedRoot.SetActive(completed);
        _newRoot.SetActive(!completed);

        _characterSpriteRenderer.sprite = PlayerManager.CurrentState.CurrentCharacterSprite;

        Highlight(false);
    }

    public void Highlight(bool isHighlighted)
    {
        _highlightedRoot.SetActive(isHighlighted);
        _notHighlightedRoot.SetActive(!isHighlighted);
    }

    public void Rotate(float rotation)
    {
        _pivot.localEulerAngles = Vector3.up * rotation;
    }
    #endregion
    #region Private
    #endregion
    #endregion
}
