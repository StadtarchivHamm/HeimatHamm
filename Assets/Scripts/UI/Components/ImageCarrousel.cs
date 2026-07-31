using System.Collections.Generic;
using TMPro;
using UniRx.Triggers;
using Unity.Samples.ScreenReader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utils;
using Wezit;

public class ImageCarrousel : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [Header("Image")]
    [SerializeField] private HorizontalScrollSnap _scrollSnap;
    [SerializeField] private CarrouselImage _imagePrefab;
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private bool _enveloppeParent;
    [Header("Controls")]
    [SerializeField] private GameObject _controlsRoot;
    [SerializeField] private GameObject _paginationPrefab;
    [SerializeField] private Transform _paginationRoot;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _selectedColor;
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _titleBGText;
    [SerializeField] private TextMeshProUGUI _titleText;
    [Header("Fullscreen")]
    [SerializeField] private FullscreenImageViewer _fullscreenImageViewer;
    #endregion

    #region Public
    #endregion

    #region Private
    private List<Image> m_paginationList = new List<Image>();
    private List<string> m_titles = new List<string>();
    private bool m_showLegendOnImage = false;
    #endregion

    #endregion

    #region Methods
    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {
        _scrollSnap.OnSelectionPageChangedEvent.AddListener(UpdatePagination);
    }
    #endregion

    #region Public
    public async void Inflate(Poi a_poi, MonoBehaviour monoBehaviour)
    {
        ResetContent();

        if (_imagePrefab == null)
        {
            return;
        }

        if (a_poi == null)
        {
            return;
        }

        await a_poi.GetRelations();

        if (a_poi.ShowPictureRelations == null || a_poi.ShowPictureRelations.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        foreach (Relation relation in a_poi.ShowPictureRelations)
        {
            CarrouselImage imageInstance = Instantiate(_imagePrefab, _contentRoot);
            imageInstance.Inflate(relation, monoBehaviour, _enveloppeParent);
                    
            if (_fullscreenImageViewer != null)
            {
                imageInstance.CarrouselImageClicked.AddListener(OnFullScreenButton);
            }
            imageInstance.UpdateHierarchy(false);
            _scrollSnap.AddChild(imageInstance.gameObject);

            if (_paginationPrefab != null)
            {
                Image pin = Instantiate(_paginationPrefab, _paginationRoot).GetComponent<Image>();
                pin.color = _defaultColor;
                m_paginationList.Add(pin);
            }

            m_titles.Add(string.IsNullOrEmpty(relation.description) ? "" : relation.CleanedDescription);
        }

        _scrollSnap.DistributePages();

        _scrollSnap.ChildObjects[0].GetComponent<CarrouselImage>().UpdateHierarchy(true);
        if (m_paginationList.Count > 0) m_paginationList[0].color = _selectedColor;
        _controlsRoot.SetActive(_scrollSnap.ChildObjects.Length > 1);
        monoBehaviour.StartCoroutine(LayoutGroupRebuilder.Rebuild(_paginationRoot.gameObject, 2));

        if (m_showLegendOnImage)
        {
            if(m_titles.Count > 0)
            {
                if (!string.IsNullOrEmpty(m_titles[0]))
                {
                    _titleBGText.gameObject.SetActive(true);
                    _titleBGText.text = m_titles[0];
                    _titleText.text = m_titles[0];
                }
                else
                {
                    _titleBGText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            _titleBGText.gameObject.SetActive(false);
        }
    }

    public void RebuildPagination(MonoBehaviour monoBehaviour)
    {
        monoBehaviour.StartCoroutine(LayoutGroupRebuilder.Rebuild(_controlsRoot));
    }
    #endregion

    #region Private
    private void ResetContent()
    {
        if(_fullscreenImageViewer)
        {
            _fullscreenImageViewer.Toggle(false);
        }

        _scrollSnap.RemoveAllChildren(out GameObject[] removedObjects);
        for (int i = 0; i < _paginationRoot.childCount; i++)
        {
            Destroy(_paginationRoot.GetChild(i).gameObject);
        }
        m_paginationList.Clear();

        foreach (GameObject carrouselImage in removedObjects)
        {
            Destroy(carrouselImage);
        }

        foreach (Transform imageTransform in _scrollSnap._screensContainer)
        {
            Destroy(imageTransform.gameObject);
        }
        m_titles.Clear();

        _titleText.text = "";
    }

    private void UpdatePagination(int page)
    {
        if (m_paginationList.Count == 0)
        {
            return;
        }

        if (m_showLegendOnImage)
        {
            if(m_titles.Count > page)
            {
                if (!string.IsNullOrEmpty(m_titles[page]))
                {
                    _titleBGText.gameObject.SetActive(true);
                    _titleBGText.text = m_titles[page];
                    _titleText.text = m_titles[page];
                }
                else
                {
                    _titleBGText.gameObject.SetActive(false);
                }
            }
        }

        for (int i = 0; i < _scrollSnap.ChildObjects.Length; i++)
        {
            _scrollSnap.ChildObjects[i].GetComponent<CarrouselImage>().UpdateHierarchy(i == page);
        }
        this.DelayRefreshHierarchy();

        for (int i = 0; i < m_paginationList.Count; i++)
        {
            m_paginationList[i].color = i == page ? _selectedColor : _defaultColor;
        }
    }

    private void OnFullScreenButton(Relation relation)
    {
        _fullscreenImageViewer.Inflate(relation);
    }
    #endregion
    #endregion
}
