using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Wezit;
using static OnlineMapsAMapSearchResult;

public class HiddenObjectView : BaseView
{
    #region Fields
    #region Serialize Fields
    [SerializeField] private ThreeDManager _threeDManager;
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private TextMeshProUGUI _objectTitle;
    [SerializeField] private TextMeshProUGUI _objectFoundAt;
    [SerializeField] private TextMeshProUGUI _objectDescription;
    #endregion Serialize Fields

    #region Public Variables
    #endregion Public Variables

    #region Private m_Variables
    #endregion Private m_Variables
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    #endregion Public

    #region Private
    protected override void InitViewContentByLang(Language language)
    {
        base.InitViewContentByLang(language);

        Poi poi = PlayerManager.CurrentState.CurrentHiddenObjectPoi;
        _threeDManager.Inflate(poi);

        _objectTitle.text = poi.CleanedTitle;
        _objectFoundAt.text = poi.CleanedSubject;
        _objectDescription.text = poi.CleanedDescription;
        _contentRoot.localPosition = Vector3.zero;
    }
    #endregion Private
    #endregion Methods
}