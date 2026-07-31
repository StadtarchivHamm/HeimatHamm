using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using SimpleJSON;

namespace Wezit
{
    public class ActivityLoader: Singleton<ActivityLoader>
    {
        public async static Task<Relation> LookForActivity(Poi a_poi, int activityIndex = 0)
        {
            await a_poi.GetRelations();

			if (a_poi.ActivityRelations?.Count == 0)
			{
				Debug.LogWarning("No activity for POI " + a_poi.title + "\n" + a_poi.pid);
			}
            return a_poi.ActivityRelations[Mathf.Min(activityIndex, a_poi.ActivityRelations.Count)];
		}

        public async static Task<bool> PoiHasActivity(Poi a_poi)
        {
            await a_poi.GetRelations();
			return a_poi.ActivityRelations?.Count > 0;
		}

		public static async Task<JSONNode> LoadActivity(Relation an_activity)
        {
			string source = an_activity.GetAssetSourceByTransformation(WezitSourceTransformation.original);

			JSONNode activitySettings = null;
			string settingsJsonString = await Utils.FileUtils.RequestTextContent(source, 5);

			if (string.IsNullOrEmpty(settingsJsonString))
			{
				Debug.LogError("Cannot load settings from " + source);
				return null;
			}
			else
			{
				activitySettings = JSON.Parse(settingsJsonString);
				return activitySettings;
			}
		}

		private const string PREFABS_PATH = "Prefabs/ActivityPrefabs/";
		public Activity InstantiateActivity(JSONNode activitySettings, Language language, Transform activityRoot)
		{
			string type = activitySettings["default"]["template.app.common.type"];
			Debug.Log("Instantiating " + type + " activity.");
            Activity instance = type switch
            {
                //ActivityType.SCRATCH => Instantiate(Resources.Load<ScratchAndReveal>(PREFABS_PATH + "ScratchAndReveal"), activityRoot),
                ActivityType.QUIZ => Instantiate(Resources.Load<Quiz>(PREFABS_PATH + "Quiz"), activityRoot),
                _ => Instantiate(Resources.Load<Activity>(PREFABS_PATH + "Default"), activityRoot),
            };
			instance.Inflate(activitySettings, language);
			return instance;
		}
    }
}
