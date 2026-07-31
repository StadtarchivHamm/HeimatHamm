using SimpleJSON;
using UniRx;
using UniRx.Async;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Wezit
{
	public static class Settings
	{
        #region Fields
        private static string TAG = "<color=red>[WezitSettings]</color>";

		private static JSONNode settingsNode;
        #endregion

        #region Properties
        public static JSONNode SettingsNode { get => settingsNode; }
        #endregion

        #region Methods
        #region Public

        public static JSONNode GetSettingArray(string arrayKey, Language language = Language.none)
		{
			language = language == Language.none ? StoreAccessor.State.Language : language;

			var value = settingsNode[language.ToString()][arrayKey];
			if (value == null)
			{
				value = settingsNode["default"][arrayKey];
			}

			return value;
		}

		public static string GetSetting(string key, Language language = Language.none)
		{
			if (string.IsNullOrEmpty(key)) return null;

			language = language == Language.none ? StoreAccessor.State.Language : language;

			var value = settingsNode[language.ToString()][key];
			if (value == null)
			{
				value = settingsNode["default"][key];
			}
			return value;
		}

		public static Color GetSettingAsColor(string key, Language language = Language.none)
		{
			string colorStr = GetSetting(key, language);

			if (string.IsNullOrEmpty(colorStr)) 
			{ 
				return Color.black;
			}
			
			return StringUtils.GetStringAsColor(colorStr);
		}

		public static bool GetSettingAsBool(string key, Language language = Language.none)
		{
			if(bool.TryParse(GetSetting(key, language), out bool value))
            {
				return value;
            }
            else
            {
				return false;
            }
		}

		public static float GetSettingAsFloat(string key, float defaultValue = 0, Language language = Language.none)
		{
			string value = GetSettingAsCleanedText(key, language);

            if (float.TryParse(value,
						   System.Globalization.NumberStyles.AllowDecimalPoint,
						   new System.Globalization.CultureInfo("en-US"),
						   out float output))
            {
				return output;
            }
			else
            {
				return defaultValue;
            }
		}

		public static string GetSettingAsCleanedText(string key, Language language = Language.none, bool replaceLineBreak = false)
		{
			return StringUtils.CleanFromWezit(GetSetting(key, language), replaceLineBreak);
		}

		public static string GetSettingAsTaggedText(string key, Language language = Language.none, bool replaceLineBreak = false)
		{
			return StringUtils.AddCustomTagsFromWezit(GetSettingAsCleanedText(key, language, replaceLineBreak));

		}

		public static WezitAssets.Asset GetSettingAsAsset(string key, Language language = Language.none)
		{
			language = language == Language.none ? StoreAccessor.State.Language : language;
			WezitAssets.Asset asset = AssetsLoader.GetAssetById(GetSettingAsCleanedText(key, language)?.Replace("wzasset://", ""));
			return asset;
		}

		public static string GetSettingAsAssetSourceByTransformation(string key, Language language = Language.none, string transformation = "default")
		{
			WezitAssets.Asset asset = GetSettingAsAsset(key, language);
			string source = "";
			if (asset != null)
			{
				source = asset.GetAssetSourceByTransformation(transformation);
			}
			return source;
		}

		public static async void DownloadSettingMedia(string key, string path = "", Language language = Language.none, string transformation = "default")
        {
            WezitAssets.Asset mediaAsset = GetSettingAsAsset(key);
            if (mediaAsset != null)
            {
                WezitAssets.File mediaFile = mediaAsset.files.Find(x => x.label == transformation);
				if (mediaFile == null)
                {
                    Debug.LogWarning("Asset is null for key " + key + " and transformation " + transformation);
                    mediaFile = mediaAsset.files.Find(x => x.label == WezitSourceTransformation.original);
                }
				if (mediaFile == null)
                {
                    Debug.LogWarning("Asset is null for key " + key);
                    return;
                }
                string mediaSource = mediaFile.uri;

                if (string.IsNullOrEmpty(mediaSource))
                {
                    Debug.LogWarning("Source is null for asset " + key);
                    return;
                }

                path = string.IsNullOrEmpty(path) ? System.IO.Path.Combine(DataGrabber.AssetsFolderPath, mediaFile.path) : path;
                if (DataGrabber.Instance.CheckDownloadNecessity(path, mediaFile.md5))
                {
					Debug.Log("Need to download asset for setting " + key);
                    await DataGrabber.Instance.DownloadFile(mediaSource, path, mediaFile.md5, mediaAsset.pid, true);
                    DataGrabber.Instance.Save();
                }
				else
				{
					Debug.Log("No need to download asset for setting " + key);
				}
            }

        }

		public static async UniTask<AudioClip> GetSettingAsAudioClip(string key, Language language = Language.none)
		{
			string source = GetSettingAsAssetSourceByTransformation(key, language);
			return await AudioUtils.GetAudioClip(source);
		}

		public static void SetImageFromSetting(RawImage rawImage, MonoBehaviour monoBehaviour, string key, Language language = Language.none, string transformation = "default", bool envelopeParent = true)
		{
			monoBehaviour.StartCoroutine(Utils.ImageUtils.SetImage(rawImage, GetSettingAsAssetSourceByTransformation(key, language, transformation), "", envelopeParent));
		}

		public static async UniTask<bool> Init(bool online)
		{
			Debug.Log(TAG + " Init");
			settingsNode = await FilesDownloader.GetSettings(online);
			return true;
		}

        #endregion
        #endregion
    }
}
