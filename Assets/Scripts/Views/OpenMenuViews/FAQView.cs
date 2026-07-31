using System;
using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;

public class FAQView : BaseView
{
	#region Fields
	public static string TAG = "<color=orange>[HomeView]</color>";

	#region Serialize Fields
	[SerializeField] private FAQQuestion _questionPrefab;
	[SerializeField] private Transform _questionsRoot;
	[SerializeField] private ContrastButton _contrastButton;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
	private string m_questionsSettingKey = "faq.questions.array";
	private string m_questionTitleSettingKey = "faq.question.title.text";
	private string m_questionContentSettingKey = "faq.question.content.text";
    #endregion Private m_Variables
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    #endregion Public

    #region Private
    protected override void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

		JSONNode questions = Wezit.Settings.GetSettingArray(m_questionsSettingKey);
		List<string> contrastButtonParagraphs = new List<string>();
		string questionTitle = "";
		string questionContent = "";


		foreach (JSONNode question in questions)
		{
			questionTitle = StringUtils.CleanFromWezit(StringUtils.AddCustomTagsFromWezit(question[m_questionTitleSettingKey]), true);
			questionContent = StringUtils.CleanFromWezit(StringUtils.AddCustomTagsFromWezit(question[m_questionContentSettingKey]), true);

			contrastButtonParagraphs.Add(questionTitle);
			contrastButtonParagraphs.Add(questionContent);

			Instantiate(_questionPrefab, _questionsRoot).Inflate(questionTitle, questionContent);
		}

		StartCoroutine(Utils.LayoutGroupRebuilder.Rebuild(_questionsRoot.gameObject));
		_contrastButton.Inflate(contrastButtonParagraphs.ToArray());
    }

    protected override void ResetViewContent()
    {
        base.ResetViewContent();

		for (int i = 2; i < _questionsRoot.childCount; i++)
		{
			Destroy(_questionsRoot.GetChild(i).gameObject);
		}
    }
    #endregion Private

    #region Internals
    #endregion Internals
    #endregion Methods
}