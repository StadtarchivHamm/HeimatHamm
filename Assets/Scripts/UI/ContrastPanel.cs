using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContrastPanel : MonoBehaviour
{
	#region Fields
	#region SerializeFields
	[SerializeField] private Image _background;
	[SerializeField] private Image _topPanelBackground;
	[SerializeField] private Image _topPanelSeparator;
	[Header("Buttons")]
	[SerializeField] private Button _closeButton;
	[SerializeField] private Image _closeButtonIcon;
	[SerializeField] private Button _fontIncreaseButton;
	[SerializeField] private Image _fontIncreaseButtonIcon;
	[SerializeField] private Button _fontDecreaseButton;
	[SerializeField] private Image _fontDecreaseButtonIcon;
	[SerializeField] private Button _colorSwapButton;
	[SerializeField] private Image _colorSwapButtonIcon;
	[Header("Text")]
	[SerializeField] private Transform _panelContent = null;
	[SerializeField] private TextMeshProUGUI _title = null;
	[SerializeField] private TextMeshProUGUI _paragraph = null;
	#endregion
	#region Private
	private string m_FontSizeStepSettingKey = "accessibility.reading.panel.font.size.step.number";
	private float m_FontSizeStep = 2;
	private bool m_colorSwapped;
    #endregion
    #endregion Fields

    #region Methods
    private void Start()
    {
        AddListeners();
    }
    #region Public
    public void Inflate(string title, string[] paragraphs)
	{
		MenuManager.Instance.SetMenuStatus(MenuManager.MenuStatus.Hidden);

		m_FontSizeStep = Wezit.Settings.GetSettingAsFloat(m_FontSizeStepSettingKey);
		m_FontSizeStep = m_FontSizeStep == 0 ? 2 : m_FontSizeStep;
		_title.text = title;
		string paragraphText = "";
        for (int i = 0; i < paragraphs.Length; i++)
        {
			paragraphText += paragraphs[i] + "\n\n";
        }
		_paragraph.text = paragraphText;
		_panelContent.position = Vector3.zero;

		if (PlayerManager.Player.ContrastTitleFontSize != -1)
		{
			_title.fontSize = PlayerManager.Player.ContrastTitleFontSize;
			_paragraph.fontSize = PlayerManager.Player.ContrastParagraphFontSize;

        }

		if (PlayerManager.Player.ContrastColorSwapped)
		{
			OnColorSwapButton();
        }

		AddListeners();
	}

	#region Private
	private void AddListeners()
	{
		RemoveListeners();

		if (_closeButton) _closeButton.onClick.AddListener(OnCloseButton);
		if (_colorSwapButton) _colorSwapButton.onClick.AddListener(OnColorSwapButton);
		if (_fontIncreaseButton) _fontIncreaseButton.onClick.AddListener(OnFontIncreaseButton);
		if (_fontDecreaseButton) _fontDecreaseButton.onClick.AddListener(OnFontDecreaseButton);
	}

	private void RemoveListeners()
	{
		if (_closeButton) _closeButton.onClick.RemoveAllListeners();
		if (_colorSwapButton) _colorSwapButton.onClick.RemoveAllListeners();
		if (_fontIncreaseButton) _fontIncreaseButton.onClick.RemoveAllListeners();
		if (_fontDecreaseButton) _fontDecreaseButton.onClick.RemoveAllListeners();
	}
	#endregion Public

	private void OnCloseButton()
	{
		MenuManager.Instance.SetMenuStatus(MenuManager.Instance.CurrentViewMenuStatus);

		PlayerManager.Player.ContrastTitleFontSize = _title.fontSize;
		PlayerManager.Player.ContrastParagraphFontSize = _paragraph.fontSize;
		PlayerManager.Player.ContrastColorSwapped = m_colorSwapped;

        PlayerManager.Player.Save();

		Destroy(gameObject);
	}

	private void OnColorSwapButton()
    {
		Color bgColor = _background.color;
		_background.color = _title.color;
		_topPanelBackground.color = _title.color;
		_topPanelSeparator.color = bgColor;

		_title.color = bgColor;
		_paragraph.color = bgColor;

		_closeButtonIcon.color = bgColor;
		_colorSwapButtonIcon.color = bgColor;
		_fontDecreaseButtonIcon.color = bgColor;
		_fontIncreaseButtonIcon.color = bgColor;

		m_colorSwapped = !m_colorSwapped;
    }

	private void OnFontIncreaseButton()
    {
		_title.fontSize += m_FontSizeStep;
		_paragraph.fontSize += m_FontSizeStep;
		StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_panelContent.gameObject));
    }

	private void OnFontDecreaseButton()
    {
		_title.fontSize -= m_FontSizeStep;
		_paragraph.fontSize -= m_FontSizeStep;
		StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_panelContent.gameObject));
    }
	#endregion Private
	#endregion Methods
}
