
using System.Collections;
using System;
using UnityEngine.Networking;

namespace Wezit
{
	public class AnalyticsService : Singleton<AnalyticsService>
	{
		/*************************************************************/
		/*********************** PROPERTIES **************************/
		/*************************************************************/

		private static string TAG = "<color=red>[AnalyticsService] </color>";

		private string url = "https://analytics.wezit.io/rest/collect";
		private string userID;
		private string userAgent = "Unity";
		private string bundleId = "";

		/*************************************************************/
		/***************** CONSTRUCTOR / DESTRUCTOR ******************/
		/*************************************************************/

		private AnalyticsService()
		{
			GenerateUId();
		}

		/*************************************************************/
		/********************** PUBLIC METHODS ***********************/
		/*************************************************************/

		public void Init()
		{
			bundleId = AppConfig.ConfigModel.bundleId;
		}

		public IEnumerator Send(Wezit.Analytics analytics)
		{
			string message = "" +
								analytics.logVersion + "," +
								analytics.timeStamp + "," +
								bundleId + "," +
								analytics.applicationVersion + "," +
								userID + "," +
								userAgent + "," +
								analytics.systemUserID + "," +
								analytics.wezitUserID + "," +
								analytics.screenID + "," +
								analytics.screenEvent + "," +
								analytics.screenEventAttrs;
			UnityEngine.Debug.Log(TAG + message);
			UnityWebRequest www = UnityWebRequest.PostWwwForm(url, message);
			yield return www.SendWebRequest();
		}

		/*************************************************************/
		/********************* PRIVATE METHODS ***********************/
		/*************************************************************/

		private void GenerateUId()
		{
			Guid guid = Guid.NewGuid();
			userID = guid.ToString();
		}
	}
}
