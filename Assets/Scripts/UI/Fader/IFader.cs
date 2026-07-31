using UnityEngine;
using UnityEngine.Events;

public abstract class IFader : MonoBehaviour
{
	#region Fields
	[SerializeField] private float _fadeTime = .2f;
	[SerializeField] private float _fadeDelay = 0f;
	[SerializeField] private bool _fadeOnEnable = true;
	[Tooltip("If true, stops the fade and set the fade value to the end")]
	[SerializeField] private bool _stopFadeOnDisable = true;
	[Tooltip("The curve to tween the fade. Set it between 0 & 1 in time, as the curve is then scaled to the duration.")]
	[SerializeField] private AnimationCurve _fadingCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	[SerializeField] private UnityEvent _onFadeEnd = null;

	private bool _isWaitingForFade = false;
	private bool _isFading = false;
	private bool _isCurveFading = false;

	private float _currentFadeWaitTime = 0f;
	private float _currentFadingTime = 0f;
	private float _currentFadingCurveTimeValue = 1f;

	#endregion Fields

	#region Methods
	#region Public
	public UnityEvent OnFadeEnd { get => _onFadeEnd; set => _onFadeEnd = value; }
	public float FadeTime { get => _fadeTime; set => _fadeTime = value; }
	public float FadeDelay { get => _fadeDelay; set => _fadeDelay = value; }
	public AnimationCurve FadingCurve { get => _fadingCurve; set => _fadingCurve = value; }
	public float CurrentFadingCurveTimeValue { get => _currentFadingCurveTimeValue; set => _currentFadingCurveTimeValue = value; }

	public virtual void StartFadingFromInit()
	{
		StartFadingFromValue();
	}

	public virtual void StartFadingFromCurrent()
	{
		_currentFadingCurveTimeValue = _currentFadingCurveTimeValue > 1 ? 1 : _currentFadingCurveTimeValue;
		StartFadingFromValue(1 - _currentFadingCurveTimeValue);
	}

	public virtual void StartFadingFromValue(float animCurveStartTimeValue = 0)
	{
		if (isActiveAndEnabled == false)
		{
			ForceToEndValue();
			return;
		}

		_currentFadingTime = FadeTime * animCurveStartTimeValue;
		_currentFadeWaitTime = 0f;

		_isFading = false;
		_isWaitingForFade = false;

		if (_fadeDelay > 0) _isWaitingForFade = true;
		else _isFading = true;

		SetFadeValue(Mathf.Clamp01(_fadingCurve.Evaluate(animCurveStartTimeValue)));
		_isCurveFading = true;
	}

	public virtual void StopFading(bool setEndValue = true)
	{
		if (_isFading)
		{
			_isFading = false;
			_isWaitingForFade = false;
			_currentFadeWaitTime = 0;
			_currentFadingTime = 0;

			if (setEndValue) ForceToEndValue();
		}
	}

	public virtual void ForceToEndValue()
	{
		SetFadeValue(_fadingCurve.Evaluate(1f));
	}
	#endregion Public

	#region MonoBehaviour
	private void OnEnable()
	{
		if (_fadeOnEnable) StartFadingFromInit();
	}

	private void OnDisable()
	{
		if (_stopFadeOnDisable) StopFading();
	}

	private void Update()
	{
		if (_isWaitingForFade)
		{
			_currentFadeWaitTime += Time.deltaTime;

			if (_currentFadeWaitTime >= _fadeDelay)
			{
				_currentFadeWaitTime = 0;
				_isWaitingForFade = false;
				_isFading = true;
			}
		}

		if (_isFading)
		{
			_currentFadingTime += Time.deltaTime;

			if (_isCurveFading)
			{
				_currentFadingCurveTimeValue = _currentFadingTime / FadeTime;
				SetFadeValue(Mathf.Clamp01(_fadingCurve.Evaluate(_currentFadingCurveTimeValue)));
			}

			if (_currentFadingTime > FadeTime)
			{
				_isFading = false;

				if (OnFadeEnd != null)
				{
					OnFadeEnd.Invoke();
				}
			}
		}
	}
	#endregion MonoBehaviour

	#region Internals
	internal abstract void SetFadeValue(float value);
	#endregion Internals
	#endregion Methods
}