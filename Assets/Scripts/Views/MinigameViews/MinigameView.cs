using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Wezit;

public class MinigameView : BaseView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] protected MinigameTimer _timer;
	[SerializeField] protected Popin _instructionPopin;
	[SerializeField] private Button _tutorialButton;
	[SerializeField] protected Popin _resultPopin;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
	protected SimpleJSON.JSONNode m_activityNode;
	protected Poi m_minigamePoi;
	protected PoiProgressionData m_poiProgressionData;
	protected Activity m_activity;
	protected bool m_started;

	private string m_successTitleSettingKey = "minigame.result.popin.title.text";
	private string m_failureTitleSettingKey = "minigame.result.popin.failure.title.text";
    #endregion Private m_Variables
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    #endregion Public

    #region Private
    protected override async void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

		MatomoAnalyticsManager.Instance.RecordGameStarted();

		m_minigamePoi = PlayerManager.CurrentState.CurrentPoi.children.Find(poi => poi.tags.Contains(Tags.MINIGAME));

		if (m_minigamePoi == null)
        {
            Debug.LogError("No minigame POI for POI " + m_minigamePoi.CleanedTitle + "\n" + m_minigamePoi.pid);
            return;
        }

		m_poiProgressionData = PlayerManager.Player.GetCurrentPoiProgression();

        if (_timer != null)
        {
            _timer.Inflate(m_poiProgressionData.MinigameBestTime);
        }

        _instructionPopin.Inflate(false, m_minigamePoi.CleanedSubject, m_minigamePoi.CleanedDescription, "");
		OnTutorialButtonClicked();

        if (await ActivityLoader.PoiHasActivity(m_minigamePoi))
		{
			Relation activityRelation = await ActivityLoader.LookForActivity(m_minigamePoi);

            m_activityNode = await ActivityLoader.LoadActivity(activityRelation);
            m_activity.Inflate(m_activityNode);

			m_activity.ActivityOver.RemoveListener(OnActivityOver);
			m_activity.ActivityOver.AddListener(OnActivityOver);
		}
	}

	protected override void ResetViewContent()
	{
		base.ResetViewContent();

		_resultPopin.Close(false);
		_instructionPopin.Close(false);

		m_started = false;
    }

	protected override void AddListeners()
	{
		base.AddListeners();

		_instructionPopin.PopinClosed.AddListener(OnInstructionPopinClosed);
		_tutorialButton.onClick.AddListener(OnTutorialButtonClicked);

		_resultPopin.PopinButtonClicked.AddListener(OnResultPopinButtonClicked);
		_resultPopin.PopinSecondaryButtonClicked.AddListener(OnResultPopinSecondaryButtonClicked);
	}

	protected override void RemoveListeners()
	{
		base.RemoveListeners();

		_instructionPopin.PopinClosed.RemoveAllListeners();
		_tutorialButton.onClick.RemoveAllListeners();

        _resultPopin.PopinButtonClicked.RemoveAllListeners();
        _resultPopin.PopinSecondaryButtonClicked.RemoveAllListeners();
    }

	private void OnTutorialButtonClicked()
	{
		_instructionPopin.Open(true);
	}

	protected virtual void OnInstructionPopinClosed()
	{
		if (!m_started)
		{
			if (_timer != null)
			{
				_timer.ToggleTimer(true);
			}

			if (m_activity != null)
			{
				m_activity.StartActivity();
			}

			m_started = true;
        }
	}

    protected void OnActivityOver()
    {
		OnActivityOver(true);
    }

	protected void OnActivityOver(bool isSuccess)
    {
        if (_timer != null && isSuccess)
        {
            _timer.ToggleTimer(false);

            if (_timer.Time <= m_poiProgressionData.MinigameBestTime || m_poiProgressionData.MinigameBestTime == 0)
            {
                m_poiProgressionData.MinigameBestTime = _timer.Time;
                PlayerManager.Player.Save();
            }
        }

        if (isSuccess && !m_poiProgressionData.MiniGameCompleted)
        {
            m_poiProgressionData.MiniGameCompleted = true;
            PlayerManager.Player.Save();
        }

        _resultPopin.Inflate(true,
							Settings.GetSettingAsCleanedText(isSuccess ? m_successTitleSettingKey : m_failureTitleSettingKey),
							"", 
							"", 
							true);
    }

	protected void OnResultPopinButtonClicked()
	{
		MenuManager.Instance.KioskStateHistory.Pop();
		MenuManager.Instance.KioskStateHistory.Pop();
		SetState(KioskState.POI_DETAILS);

        _resultPopin.Close(false);
	}

	protected void OnResultPopinSecondaryButtonClicked()
	{
		InitViewContentByLang(StoreAccessor.State.Language);
        _resultPopin.Close(false);
    }
	#endregion Private

	#region Internals
	#endregion Internals
	#endregion Methods
}