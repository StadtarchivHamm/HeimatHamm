using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContrastButton : MonoBehaviour
{
	#region Fields
	internal Button m_Button;
	[SerializeField] private Image _icon;
	[SerializeField] private ContrastPanel _contrastPanelPrefab;
	[Header("Self-inflation")] 
	[SerializeField] private Transform _panelRoot;
	[SerializeField] private TextMeshProUGUI _titleTMPro;
	[SerializeField] private List<TextMeshProUGUI> _paragraphsTMPros;
	
	private Transform m_ContrastPanelRoot = null;
	private string m_title = null;
	private string[] m_paragraphs = null;
	private bool m_initialized;
	#endregion Fields

	#region Methods
	#region MonoBehaviour
	internal void Awake()
	{
		m_Button = GetComponent<Button>();
		AddListeners();
	}
	#endregion MonoBehaviour

	#region Public
	public void Inflate(string title, string[] paragraphs, Transform panelRoot)
	{
		gameObject.SetActive(!PlayerManager.CurrentState.IsAudioDescription);
		m_ContrastPanelRoot = panelRoot;
		m_title = title;
		m_paragraphs = paragraphs.Length == 0 ? m_paragraphs : paragraphs;
		m_initialized = true;
	}

	public void Inflate(string[] paragraphs)
    {
        gameObject.SetActive(!PlayerManager.CurrentState.IsAudioDescription);
        m_paragraphs = paragraphs.Length == 0 ? m_paragraphs : paragraphs;
		m_initialized = true;
	}

	public void AddListeners()
	{
		RemoveListeners();

		if (m_Button) m_Button.onClick.AddListener(OnButtonClick);
	}

	public void RemoveListeners()
	{
		if (m_Button) m_Button.onClick.RemoveListener(OnButtonClick);
	}
	#endregion Public

	#region Private
	private void OnButtonClick()
	{
		if (_panelRoot != null && (!m_initialized || _titleTMPro.text != m_title))
		{
			List<string> paragraphs = new List<string>();
			foreach (TextMeshProUGUI paragraphTMPro in _paragraphsTMPros)
			{
				paragraphs.Add(paragraphTMPro.text);
			}
			Inflate(_titleTMPro.text, paragraphs.ToArray(), _panelRoot);
		}

		Instantiate(_contrastPanelPrefab, m_ContrastPanelRoot).Inflate(m_title, m_paragraphs);
	}
	#endregion Private
	#endregion Methods
}
