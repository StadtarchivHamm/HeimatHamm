using UnityEngine;
using Wezit;

public class TouchMinigameView : MinigameView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private TouchActivity _touchActivity;
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
		m_activity = _touchActivity;
		base.InitViewContentByLang(language);
	}
	#endregion Private

	#region Internals
	#endregion Internals
	#endregion Methods
}