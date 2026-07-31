using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SliderWithPointerEvents : Slider
{
    #region Properties
    public UnityEvent onSliderPointerDown = new UnityEvent();
    public UnityEvent onSliderPointerUp = new UnityEvent();
    #endregion

    #region Methods
    #region Monobehaviours
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        onSliderPointerDown?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        onSliderPointerUp?.Invoke();
    }
    #endregion
    #endregion
}
