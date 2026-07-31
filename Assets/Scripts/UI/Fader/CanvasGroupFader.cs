using UnityEngine;

public class CanvasGroupFader : IFader
{
	#region Fields
	[SerializeField] private CanvasGroup _canvasGroup = null;
	#endregion Fields

	#region Methods
	#region MonoBehavior
	private void Awake()
	{
		if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
	}
	#endregion MonoBehavior

	#region Internals
	internal override void SetFadeValue(float value)
	{
		if (_canvasGroup)
		{
			_canvasGroup.alpha = value;
			_canvasGroup.interactable = _canvasGroup.alpha > 0;
		}
	}
	#endregion Internals
	#endregion Methods
}
