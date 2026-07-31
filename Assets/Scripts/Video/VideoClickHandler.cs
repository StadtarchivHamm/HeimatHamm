using UnityEngine;
using UnityEngine.EventSystems;

public class VideoClickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	bool click;

	public delegate void ClickAction();
	public event ClickAction OnClicked;

	public void OnPointerDown(PointerEventData eventData)
	{
		click = true;
		OnClicked();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		click = false;
	}

	public bool IsClicked()
	{
		return click;
	}
}
