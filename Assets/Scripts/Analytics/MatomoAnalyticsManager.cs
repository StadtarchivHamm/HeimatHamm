using System;
using System.Collections;
using Lumpn.Matomo;
using UnityEngine;
using UnityEngine.Networking;

public class MatomoAnalyticsManager : Singleton<MatomoAnalyticsManager>
{
	private static string TAG = "<color=red>[MatomoAnalyticsManager]</color>";

	#region Const
	private const string TRACKER_DATA_PATH = "Analytics/MatomoTrackerData";
	#endregion Const

	#region Fields
	private MatomoTrackerData _trackerData = null;
	private MatomoTracker _tracker = null;
	private MatomoSession _currentSession = null;

	private DateTime m_sessionStartTime;
	#endregion Fields

	#region Properties
	#endregion Properties

	#region Methods
	#region MonoBehaviour
	protected override void Awake()
	{
		base.Awake();

		if (AppManager.Instance.LoadingOver)
		{
			InitTracker();
		}
		else
		{
			AppManager.Instance.OnLoadingOver.AddListener(InitTracker);
		}
	}
	#endregion MonoBehaviour

	#region Internals
	private void InitTracker()
	{
		AppManager.Instance.OnLoadingOver.RemoveListener(InitTracker);

		_trackerData = Resources.Load<MatomoTrackerData>(TRACKER_DATA_PATH);

		if (_trackerData == null)
		{
			Debug.LogError("Couldn't find Matomo Tracker Data at " + TRACKER_DATA_PATH);
			return;
		}

		string matomoUrl = Wezit.Settings.GetSetting("analytics.matomo.url");
		if (!string.IsNullOrEmpty(matomoUrl) && int.TryParse(Wezit.Settings.GetSetting("analytics.matomo.website.id"), out int websiteId))
		{
			_trackerData.MatomoUrl = matomoUrl;
			_trackerData.WebsiteUrl = Wezit.Settings.GetSetting("analytics.matomo.website.url");
			_trackerData.WebsiteId = websiteId;
		}
		else
		{
			Debug.LogError(TAG + " Invalid or empty Matomo config (Wezit settings) — analytics will target the placeholder host.");
		}

		Debug.Log(TAG + " InitTracker ---------------- \n" +
			" MatomoUrl : " + _trackerData.MatomoUrl + "\n" +
			" WebsiteUrl : " + _trackerData.WebsiteUrl + "\n" +
			" WebsiteId : " + _trackerData.WebsiteId
			);

		_tracker = _trackerData.CreateTracker();

		// Open a session immediately so subsequent Record* calls have a live session.
		RecordAppOpen();
	}

	private IEnumerator RecordAction(string actionTitle, string actionUrl, float time)
	{
		UnityWebRequestAsyncOperation operation = _currentSession.Record(actionTitle, actionUrl, time);
		yield return operation;

		Debug.Log("Response for Matomo action " + actionTitle + " : " + operation.webRequest.responseCode);
	}
	#endregion Internals

	#region Public
	public void StartNewPlayerSession()
	{
		if (_tracker == null)
		{
			Debug.LogError("Tracker not initialized !");
			return;
		}
		string playerId = System.Guid.NewGuid().ToString();
		_currentSession = _tracker.CreateSession(playerId);
		m_sessionStartTime = DateTime.Now;
	}

	public void RecordAppOpen()
	{
		StartNewPlayerSession();

		if (_currentSession == null)
		{
			Debug.LogError("Session not initialized !");
			return;
		}

		StartCoroutine(RecordAction("App Open", "start", 0));
	}

	public void RecordCurrentView(string currentView)
	{
		if (_currentSession == null)
		{
			Debug.LogError("Session not initialized !");
			return;
		}

		StartCoroutine(RecordAction("Opened " + currentView + " view", "view/" + currentView, 0));
	}

	public void RecordBackHome(string previousView)
	{
		if (_currentSession == null)
		{
			Debug.LogError("Session not initialized !");
			return;
		}

		StartCoroutine(RecordAction("Returned home from: " + previousView, "home/" + previousView, 0));
	}

	public void RecordSessionLanguage(Language language)
	{
		if (_currentSession == null)
		{
			Debug.LogError("Session not initialized !");
			return;
		}

		StartCoroutine(RecordAction($"Language selected: {language}", $"language/{language}", 0));
    }

    public void RecordTourStarted(string tourName, string tourPid)
    {
        if (_currentSession == null)
        {
            Debug.LogError("Session not initialized !");
            return;
        }

        StartCoroutine(RecordAction($"Tour started: {tourName}", $"tourStarted/{tourName} ({tourPid})", 0));
    }

    public void RecordPoiVisited(string tourName, string pid, string poiName)
    {
        if (_currentSession == null)
        {
            Debug.LogError("Session not initialized !");
            return;
        }

        StartCoroutine(RecordAction($"Poi visited: {poiName}", $"{tourName}/poiStarted/{poiName} ({pid})", 0));
    }

    public void RecordARSessionStarted()
    {
        if (_currentSession == null)
        {
            Debug.LogError("Session not initialized !");
            return;
        }

        StartCoroutine(RecordAction("AR session started", "arSessionStarted", 0));
    }

    public void RecordARSessionStartedForTour(string tourPid, string tourName)
    {
        if (_currentSession == null)
        {
            Debug.LogError("Session not initialized !");
            return;
        }

        StartCoroutine(RecordAction("AR session started in Tour", $"arSessionStartedForTour/{tourPid} ({tourName})" , 0));
    }

    public void RecordSecretARSessionStarted()
    {
        if (_currentSession == null)
        {
            Debug.LogError("Session not initialized !");
            return;
        }

        StartCoroutine(RecordAction("Secret AR session started", "secretArSessionStarted", 0));
    }

    public void RecordGameStarted()
    {
        if (_currentSession == null)
        {
            Debug.LogError("Session not initialized !");
            return;
        }

        StartCoroutine(RecordAction("Game started", "gameStarted", 0));
    }
    #endregion Public
    #endregion Methods
}