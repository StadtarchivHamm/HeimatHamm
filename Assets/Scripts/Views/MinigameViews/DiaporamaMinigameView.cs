using UnityEngine;
using UnityEngine.UI;
using Utils;

public class DiaporamaMinigameView : MinigameView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private RawImage _diaporamaBackground;
	[SerializeField] private ImageCarrousel _carrousel;
	[SerializeField] private Button _continueButton;
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

		ImageUtils.LoadRefImage(_diaporamaBackground, this, m_minigamePoi, fillParent:false);
		_carrousel.Inflate(m_minigamePoi, this);
	}

	protected override void AddListeners()
	{
		base.AddListeners();

		_continueButton.onClick.AddListener(OnActivityOver);
	}

	protected override void RemoveListeners()
	{
		base.RemoveListeners();

        _continueButton.onClick.RemoveAllListeners();
    }
	#endregion Private

	#region Internals
	#endregion Internals
	#endregion Methods
}