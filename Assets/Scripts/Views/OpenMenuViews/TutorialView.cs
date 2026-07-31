using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class TutorialView : BaseView
{
	#region Fields
	public static string TAG = "<color=orange>[HomeView]</color>";

	#region Serialize Fields
	[SerializeField] private TutorialPopin _tutorialPopin;
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
		base.InitViewContentByLang(language);

		_tutorialPopin.TogglePopin(true);
	}
	#endregion Private

	#region Internals
	#endregion Internals
	#endregion Methods
}