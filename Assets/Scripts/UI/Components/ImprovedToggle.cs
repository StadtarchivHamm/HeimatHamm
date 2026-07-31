using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImprovedToggle : Toggle
{
    #region Fields
    [SerializeField] private Image _toggleImage;
    [SerializeField] private Sprite _offSprite;
    [SerializeField] private Color _offColor = Color.white;
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Color _onColor = Color.white;

    [SerializeField] private UnityEvent _onToggleOn = new UnityEvent();
    [SerializeField] private UnityEvent _onToggleOff = new UnityEvent();

    private bool _wasOn = false;
	#endregion Fields

	#region Properties
	#endregion Properties

	#region Methods
    protected override void Awake()
    {
        base.Awake();
        onValueChanged.AddListener(OnToggleValueChanged);
        OnToggleValueChanged(isOn);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnToggleValueChanged(isOn);
    }

    public void ToggleGraphic(bool isOn)
    {
        if (isOn)
        {
            if (_toggleImage != null && _onSprite != null)
            {
                _toggleImage.sprite = _onSprite;
            }
            if (targetGraphic != null)
            {
                targetGraphic.color = _onColor;
            }
        }
        else
        {
            if (_toggleImage != null && _offSprite != null)
            {
                _toggleImage.sprite = _offSprite;
            }
            if (targetGraphic != null)
            {
                targetGraphic.color = _offColor;
            }
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        ToggleGraphic(isOn);
        if (isOn)
        {
            _onToggleOn.Invoke();
        }
        else
        {
            _onToggleOff.Invoke();
        }
    }
	#endregion Methods
}