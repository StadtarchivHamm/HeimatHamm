using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using Utils;

/**
 * Created by Willy J.
 * !!!IMPORTANT!!! Make sure your content is center anchored and not in stretch. If not, it could wield weird results.
 */

public class PinchableScrollRect : ScrollRect
{
	#region Fields
    [SerializeField]
	private float _minZoom = 1f;

    [SerializeField]
	private float _maxZoom = 10f;

	[SerializeField] 
	private bool _dezoomElastic = false;

	private Rect _thisRect;
	private Rect _contentRect;

	private float _currentZoom = 1f;
	private float _currentLerpedZoom = 1f;
	private float _zoomLerpSpeed = 10f;
	private float _mouseWheelSensitivity = .1f;

	private float _startPinchDist;
	private float _startPinchZoom;
	private bool _isPinching = false;
	private bool _blockPan = false;

	private Vector2 _forcePivotPosition;
	private bool _forceXPivotOnCenter;
	private bool _forceYPivotOnCenter;

	private bool _recomputePivot = true;
	#endregion Fields

    #region Properties
    public float MinZoom { get { return _minZoom; } set { _minZoom = value; } }
    public float MaxZoom { get { return _maxZoom; } set { _maxZoom = value; } }
    public float CurrentZoom { get { return _currentZoom; } }
	public float CurrentLerpedZoom { get { return _currentLerpedZoom; } }

	public bool RecomputePivot { get { return _recomputePivot; } set { _recomputePivot = value; } }
	#endregion Properties

	#region Methods
	#region Public
	public void Init(bool fitInParent)
	{
		_thisRect = GetComponent<RectTransform>().rect;

		_contentRect = content.rect;

		float targetScaleX = (float)Math.Round(_thisRect.width / _contentRect.width, 4);
		float targetScaleY = (float)Math.Round(_thisRect.height / _contentRect.height, 4);
		float targetScale = fitInParent ? Mathf.Min(targetScaleX, targetScaleY) : Mathf.Max(targetScaleX, targetScaleY);

		_currentZoom = targetScale;
		_minZoom = targetScale;
		if (_maxZoom < _minZoom) _maxZoom = _minZoom;
		SetContentScale(new Vector3(_currentZoom, _currentZoom, _currentZoom));

		SetPivotOnCenter();
		content.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
	}

	public override void OnScroll(PointerEventData data)
	{
		if (Mathf.Abs(data.scrollDelta.y) > float.Epsilon)
		{
			_currentZoom *= 1 + data.scrollDelta.y * _mouseWheelSensitivity;
			_currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);

			_forcePivotPosition = InitPivotPosition(content, (Vector2)Input.mousePosition);
		}
	}

	public void SetCurrentZoom(float a_zoom)
    {
		_currentZoom = Mathf.Clamp(a_zoom, _minZoom, _maxZoom);
	}
	#endregion Public

	#region MonoBehavior
	protected override void Awake()
	{
		Input.multiTouchEnabled = true;
		_thisRect = GetComponent<RectTransform>().rect;
	}

	private void Update()
	{
        if (Application.isPlaying)
        {
            if (Input.touchCount == 2)
            {
                if (!_isPinching && IsPinchEnabled())
                {
                    _isPinching = true;
                    OnPinchStart();
                }
                OnPinch();
            }
            else
            {
                if (_isPinching && _dezoomElastic) StartCoroutine(KeepsInLimits());

                _isPinching = false;
                if (Input.touchCount == 0)
                {
                    _blockPan = false;
                }
            }

			if(RecomputePivot)
				SetPivotOnCenter();

            if (Mathf.Abs(content.localScale.x - _currentZoom) > 0.001f)
            {
				_currentLerpedZoom = Mathf.Lerp(content.localScale.x, 1f * _currentZoom, _zoomLerpSpeed * Time.deltaTime);
				SetContentScale(Vector3.one * _currentLerpedZoom);
			}
                
        }
	}

	private void SetPivotOnCenter()
	{
		_forceXPivotOnCenter = ((Math.Round(content.rect.width * content.localScale.x, 4)) <= _thisRect.width || _currentZoom <= _minZoom);
		_forceYPivotOnCenter = ((Math.Round(content.rect.height * content.localScale.y, 4)) <= _thisRect.height || _currentZoom <= _minZoom);
		RectTransformUtils.SetPivot(content, new Vector2(_forceXPivotOnCenter ? 0.5f : _forcePivotPosition.x, _forceYPivotOnCenter ? 0.5f : _forcePivotPosition.y));
	}
	#endregion MonoBehavior

	#region Private
	private IEnumerator KeepsInLimits()
	{
		this.movementType = MovementType.Clamped;
		yield return new WaitForSeconds(0.25f);
		this.movementType = MovementType.Clamped;
	}

	private bool IsPinchEnabled()
	{
		Vector2 localTouchPos0;
		Vector2 localTouchPos1;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.GetTouch(0).position, null, out localTouchPos0);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.GetTouch(1).position, null, out localTouchPos1);

		return (GetComponent<RectTransform>().rect.Contains(localTouchPos0) && GetComponent<RectTransform>().rect.Contains(localTouchPos1));
	}

	private void OnPinchStart()
	{
		_startPinchDist = RectTransformUtils.CalculateDistanceBetweenPoints(content, Input.GetTouch(0).position, Input.GetTouch(1).position) * content.localScale.x;
		_startPinchZoom = _currentZoom;

		_blockPan = true;
	}

	private void OnPinch()
	{
		if (!_isPinching) return;

		float currentPinchDist = RectTransformUtils.CalculateDistanceBetweenPoints(content, Input.GetTouch(0).position, Input.GetTouch(1).position) * content.localScale.x;
		_currentZoom = (currentPinchDist / _startPinchDist) * _startPinchZoom;
		_currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);

		_forcePivotPosition = InitPivotPosition(content, (Input.GetTouch(0).position + Input.GetTouch(1).position) * 0.5f);
	}

	private Vector2 InitPivotPosition(RectTransform _content, Vector2 startZoomScreenPosition)
	{
		Vector2 zoomCenterPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_content, startZoomScreenPosition, null, out zoomCenterPoint);
		Vector2 pivotPosition = new Vector3(_content.pivot.x * _content.rect.size.x, _content.pivot.y * _content.rect.size.y);
		Vector2 posFromBottomLeft = pivotPosition + zoomCenterPoint;

		return new Vector2(posFromBottomLeft.x / _content.rect.width, posFromBottomLeft.y / _content.rect.height);
	}

	protected override void SetContentAnchoredPosition(Vector2 position)
	{
		if (_isPinching || _blockPan) return;
		base.SetContentAnchoredPosition(position);
	}

	private void SetContentScale(Vector3 scaleValue)
	{
		content.localScale = scaleValue;
	}
	#endregion Private
	#endregion Methods
}