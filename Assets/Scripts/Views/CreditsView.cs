/**
 * Created by Willy
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;

public class CreditsView : BaseView
{
	#region Fields
	public static string TAG = "<color=orange>[CreditsManager]</color>";
	public static string WEZIT_TAG = "credits";

	#region Serialize Fields
	[SerializeField] private Image _uIBackground = null;
	[SerializeField] private TextMeshProUGUI _titleText = null;
	[SerializeField] private TextMeshProUGUI _descriptionText = null;
	[SerializeField] private Button _closeButton = null;
	// [SerializeField] private CanvasGroupFader _faderOut;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
	private Wezit.Poi m_WzPoiData = null;
	#endregion Private m_Variables
	#endregion Fields

	#region Properties
	#endregion Properties

	#region Methods
	#region Public
	#endregion Public

	#region MonoBehavior
	#endregion MonoBehavior

	#region Internals
	protected override void OnFadeEndView()
	{

	}
	#endregion Internals

	#region Private
	private void AddListeners()
	{
		RemoveListeners();

		if (_closeButton) _closeButton.onClick.AddListener(OnCloseButton);
		// if (_faderOut) _faderOut.OnFadeEnd.AddListener(OnFadeOutEnd);
	}

	private void RemoveListeners()
	{
		if (_closeButton) _closeButton.onClick.RemoveAllListeners();
		// if (_faderOut) _faderOut.OnFadeEnd.RemoveAllListeners();
	}

	private void OnFadeOutEnd()
	{
		HideView();
	}

	private void OnCloseButton()
	{
		// if (_faderOut) _faderOut.StartFading();
		ViewManager.Instance.GoBack();
	}

	private void ResetViewContent()
	{
		m_WzPoiData = null;
		if (_titleText) _titleText.text = "";
		if (_descriptionText) _descriptionText.text = "";
	}

	private void InitViewContentByLang(Language language)
	{
		ResetViewContent();
		m_WzPoiData = WezitDataUtils.GetWezitPoiByTag(language, WEZIT_TAG);

		if (m_WzPoiData == null) return;

		if (_titleText) _titleText.text = StringUtils.CleanFromWezit(m_WzPoiData.title.ToUpper());
		if (_descriptionText) _descriptionText.text = StringUtils.CleanFromWezit(m_WzPoiData.description);
	}
	#endregion Private
	#endregion Methods
}