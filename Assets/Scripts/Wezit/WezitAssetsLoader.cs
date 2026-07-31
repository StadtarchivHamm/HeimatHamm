using UniRx;
using UniRx.Async;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wezit
{
	public class AssetsLoader
	{
		/*************************************************************/
		/*********************** PROPERTIES **************************/
		/*************************************************************/

		private static string TAG = "<color=purple>[WezitAssetsLoader]</color>";

		private static string m_assetsJsonString;
		private static List<WezitAssets.Asset> m_assets;

		/*************************************************************/
		/********************** GETTER / SETTER **********************/
		/*************************************************************/

		public static string AssetsJsonString { get => m_assetsJsonString; }
		public static List<WezitAssets.Asset> Assets { get => m_assets; }

		public static WezitAssets.Asset GetAssetById(string assetPid)
		{
			return m_assets.Find(x => x.pid == assetPid);
		}

		public static List<WezitAssets.Asset> GetAssetsForTour(string tourPid)
		{
			JObject jObject = JObject.Parse("{\"assets\": " + m_assetsJsonString + "}");

			IEnumerable<JToken> tourAssetsJtokens = jObject.SelectTokens(@"$.assets[?(@.use =~ /^.*" + tourPid + "(.*)$/)]");

			IEnumerable<JToken> tourActivitiesPids = jObject.SelectTokens(@"$.assets[?(@.use =~ /^.*" + tourPid + "(.*)$/ && @.synchronise.defaultmode == 'activity')].pid");
			List<JToken> tourActivitiesAssets = new List<JToken>();
			foreach (JToken activityPid in tourActivitiesPids)
			{
				IEnumerable<JToken> activityAssets = jObject.SelectTokens(@"$.assets[?(@.use =~ /^.*" + activityPid.ToString() + "(.*)$/)]");
				tourActivitiesAssets.AddRange(activityAssets);
			}

			if (tourAssetsJtokens.Count() + tourActivitiesAssets.Count > 0)
			{
				// Concatenate all assets jtokens into one usable json
				string newJson = "[";
				foreach (JToken item in tourAssetsJtokens)
				{
					newJson += item + ", ";
				}
				foreach (JToken item in tourActivitiesAssets)
				{
					newJson += item + ", ";
				}
				newJson = newJson.Remove(newJson.Length - 2);
				newJson += "]";

				// Then parse it into a list of assets
				List<WezitAssets.Asset> tourAssets = JsonConvert.DeserializeObject<List<WezitAssets.Asset>>(newJson);
				tourAssets = tourAssets.Distinct().ToList();

				return tourAssets;
			}
			else return null;
		}

		public static List<WezitAssets.Asset> GetAllSettingsAssets()
		{
			JObject jObject = JObject.Parse("{\"assets\": " + m_assetsJsonString + "}");

			IEnumerable<JToken> tourAssetsJtokens = jObject.SelectTokens(@"$.assets[?(@.use =~ /^.*setting(.*)$/)]");

			if (tourAssetsJtokens.Count() > 0)
			{
				// Concatenate all assets jtokens into one usable json
				string newJson = "[";
				foreach (JToken item in tourAssetsJtokens)
				{
					newJson += item + ", ";
				}
				newJson = newJson.Remove(newJson.Length - 2);
				newJson += "]";

				// Then parse it into a list of assets
				List<WezitAssets.Asset> tourAssets = JsonConvert.DeserializeObject<List<WezitAssets.Asset>>(newJson);
				tourAssets = tourAssets.Distinct().ToList();

				return tourAssets;
			}
			else return null;
		}

		/*************************************************************/
		/********************** PUBLIC METHODS ***********************/
		/*************************************************************/

		public static async UniTask<bool> Init(bool online)
		{
			Debug.Log(TAG + " Init");
			m_assetsJsonString = await Wezit.FilesDownloader.GetAssets(online);
			m_assets = JsonConvert.DeserializeObject<List<WezitAssets.Asset>>(m_assetsJsonString);
			return true;
		}
	}
} // End namespace Wezit
