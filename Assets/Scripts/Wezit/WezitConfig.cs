using System.IO;
using UniRx.Async;
using UnityEngine;

namespace Wezit
{
	public static class Config
	{
		/*************************************************************/
		/*********************** PROPERTIES **************************/
		/*************************************************************/

		private static string TAG = "<color=red>[WezitConfig]</color>";
		private static string CONFIG_FILE_NAME = "wezit_config.json";

		private static ConfigModel wezitConfig;

		/*************************************************************/
		/********************** GETTER / SETTER **********************/
		/*************************************************************/

		public static ConfigModel WezitConfig { get => wezitConfig; }

		/*************************************************************/
		/********************** PUBLIC METHODS ***********************/
		/*************************************************************/

		public static async UniTask Init()
		{
			Debug.Log(TAG + " Init");
			wezitConfig = JsonUtility.FromJson<ConfigModel>(await LoadConfigFile());
		}

		private static async UniTask<string> LoadConfigFile()
		{
			string configJsonUrl = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);
			string configJsonString = await Utils.FileUtils.RequestTextContent(configJsonUrl, 5);

			if (string.IsNullOrEmpty(configJsonString))
			{
				Debug.LogError(TAG + "Can not load Wezit configuration file");
				return null;
			}
			else
			{
				Debug.Log(TAG + "ConfigFile loaded");
				return configJsonString;
			}
		}
	}
}
