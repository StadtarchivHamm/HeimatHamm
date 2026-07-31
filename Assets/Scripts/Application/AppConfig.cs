using SimpleJSON;
using UniRx.Async;
using System.IO;
using UnityEngine;

public static class AppConfig
{
    #region Fields
    private static string TAG = "<color=red>[AppConfig]</color>";
	private static string CONFIG_FILE_NAME = "config.json";

	private static AppConfigModel configModel;
    #endregion

    #region Properties
    public static AppConfigModel ConfigModel { get => configModel; }
    #endregion

    #region Methods
    #region Public
    public static async UniTask<bool> Init()
	{
		Debug.Log(TAG + " Init");
		JSONNode result = await LoadConfigFile();
		configModel = JsonUtility.FromJson<AppConfigModel>(result["app"].ToString());

		//Unity.FileDebugConfig.Instance.Init(result);
		return true;
	}

	public static bool ShowVersion()
	{
		return Wezit.Settings.GetSettingAsBool(configModel.showVersionSettingKey, Language.fr_FR);
	}
    #endregion

    #region Private
    private static async UniTask<JSONNode> LoadConfigFile()
	{
		string configJsonUrl = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);
		string configJsonString = await Utils.FileUtils.RequestTextContent(configJsonUrl, 5);

		if (string.IsNullOrEmpty(configJsonString))
		{
			Debug.LogError(TAG + "Can not load configuration file");
			return null;
		}
		else
		{
			Debug.Log(TAG + "ConfigFile loaded");
			return SimpleJSON.JSON.Parse(configJsonString);
		}
	}
    #endregion
    #endregion
}
