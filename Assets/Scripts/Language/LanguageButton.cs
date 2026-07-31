using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LanguageButton : MonoBehaviour
{
	#region Fields
	[SerializeField] private Button _button;
	[SerializeField] private Language _language;
	[SerializeField] private Image _background;
	[SerializeField] private TextMeshProUGUI _text;
	[SerializeField] private Color _blue;
	[SerializeField] private Color _white;
	#endregion Fields

	#region Properties
	public UnityEvent<Language, bool> LanguageButtonClicked = new UnityEvent<Language, bool>();
	public Language Language {  get { return _language; } }
    #endregion

    #region Methods
    #region MonoBehaviour
    internal void Awake()
	{
		AddListeners();
	}
	#endregion MonoBehaviour

	#region Public
	public void Init()
    {
		AddListeners();

		if (_background != null)
		{
			_background.color = StoreAccessor.State.Language == _language ? _white : _blue;
			_text.color = StoreAccessor.State.Language == _language ? _blue : _white;
		}
    }
	#endregion Public

	#region Private
	private void OnButtonClicked()
	{
		LanguageButtonClicked?.Invoke(_language, true);
	}

	private void AddListeners()
	{
		RemoveListeners();
		_button.onClick.AddListener(OnButtonClicked);
	}

	private void RemoveListeners()
	{
		_button.onClick.RemoveAllListeners();
	}
	#endregion Private
	#endregion Methods
}
