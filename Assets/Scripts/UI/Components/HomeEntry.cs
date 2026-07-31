using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Utils;
using Unity.Samples.ScreenReader;

public class HomeEntry : MonoBehaviour
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private TextMeshProUGUI _titleText;
	[SerializeField] private TextMeshProUGUI _descriptionText;
	[SerializeField] private RawImage _imageBackground;
	[SerializeField] private RawImage _imageCharacter;
	[SerializeField] private Button _tourButton;
	#endregion Serialize Fields

	#region Public Variables
	public UnityEvent<Wezit.Tour, bool, bool> HomeEntryClicked = new UnityEvent<Wezit.Tour, bool, bool>();
	#endregion Public Variables

	#region Private m_Variables
	private Wezit.Tour m_tourData;

	private bool m_isAudioDescription;
	private bool m_isEasyLanguage;
	#endregion Private m_Variables
	#endregion Fields

	#region Properties
	#endregion Properties

	#region Methods
	#region MonoBehaviour
	#endregion MonoBehaviour

	#region Public
	public void Inflate(Wezit.Tour a_tour, bool isAudioDescription, bool isEasyLanguage)
	{
		ResetData();

        m_isAudioDescription = isAudioDescription;
        m_isEasyLanguage = isEasyLanguage;

        m_tourData = a_tour;
		if (a_tour == null)
		{
			return;
        }

        string[] splitTitle = a_tour.CleanedTitle.Split('|');
        _titleText.text = splitTitle.Length > 1 ? splitTitle[0] : a_tour.CleanedTitle;
		_titleText.GetComponent<AccessibleText>().SetLabel(m_tourData.CleanedTitle);
        _tourButton.GetComponent<AccessibleButton>().value = m_tourData.CleanedTitle;

        if (_descriptionText != null)
		{
			_descriptionText.text = m_tourData.CleanedSubject;
            _descriptionText.GetComponent<AccessibleText>().SetLabel(_descriptionText.text);
        }
		if (_imageBackground != null) ImageUtils.LoadImage(_imageBackground, this, m_tourData);
		if (_imageCharacter != null) ImageUtils.LoadRefImage(_imageCharacter, this, m_tourData);
	}

	public void ResetData()
	{
		m_tourData = null;

		_titleText.text = "";
        if (_descriptionText != null) _descriptionText.text = "";
        if (_imageBackground != null) ImageUtils.ResetImage(_imageBackground);
        if (_imageCharacter != null) ImageUtils.ResetImage(_imageCharacter);
	}
	#endregion Public

	#region Private
	private void OnEnable()
	{
		AddListeners();
	}

	private void OnDisable()
	{
		RemoveListeners();
	}

	private void AddListeners()
	{
		RemoveListeners();

		if (_tourButton != null)
		{
			_tourButton.onClick.AddListener(OnTourButtonClick);
		}
	}

	private void RemoveListeners()
    {
        if (_tourButton != null)
        {
            _tourButton.onClick.RemoveListener(OnTourButtonClick);
        }
    }

	private void OnTourButtonClick()
	{
		HomeEntryClicked?.Invoke(m_tourData, m_isAudioDescription, m_isEasyLanguage);
	}
	#endregion Private
	#endregion Methods
}
