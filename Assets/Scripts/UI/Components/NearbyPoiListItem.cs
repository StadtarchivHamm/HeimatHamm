using UnityEngine;
using UnityEngine.Events;

public class NearbyPoiListItem : PoiListItem
{
    #region Fields
    #region SerializeFields
    #endregion
    #region Private
    #endregion
    #endregion

    #region Properties
    public UnityEvent<Wezit.Poi> NearbyPoiClicked = new UnityEvent<Wezit.Poi>();
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion

    #region Public
    #endregion
        #region Private
    protected override void OnButtonClicked()
    {
        NearbyPoiClicked?.Invoke(m_poi);
    }
    #endregion
    #endregion
}
