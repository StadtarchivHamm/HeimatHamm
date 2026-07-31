using UnityEngine;
using Wezit;

public class QuizView : MinigameView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private Quiz _quiz;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
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
		m_activity = _quiz;

		// For the quiz we want to know if the activity was actually a success. Here we're removing the non-bool OnActivityOver
        m_activity.ActivityOver.RemoveListener(OnActivityOver);
		
		// And it so happens that we have such an event ready with a bool parameter
		_quiz.QuizOver.RemoveListener(OnActivityOver);
		_quiz.QuizOver.AddListener(OnActivityOver);

        base.InitViewContentByLang(language);
	}
	#endregion Private

	#region Internals
	#endregion Internals
	#endregion Methods
}