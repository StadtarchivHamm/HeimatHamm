using UnityEngine;
using UnityEngine.UI;

public class GraphicFader : IFader
{
	#region Fields
	[SerializeField] private Graphic _graphic = null;
    #endregion Fields

    #region Methods
    #region Monobehaviours
    private void Awake()
    {
		if (_graphic == null) _graphic = GetComponent<Graphic>();
    }
    #endregion
    #region Internals
    internal override void SetFadeValue(float value)
	{
		if (_graphic == null) return;
		
		Color color = _graphic.color;
		color[3] = value;
		_graphic.color = color;
	}

	public void SetAnimCurve(AnimationCurve animationCurve = null)
	{
		if (animationCurve == null) animationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
		FadingCurve = animationCurve;
	}
	#endregion Internals
	#endregion Methods
}
