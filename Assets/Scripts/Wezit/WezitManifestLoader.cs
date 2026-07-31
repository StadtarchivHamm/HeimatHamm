using UniRx;
using UniRx.Async;
using UnityEngine;

namespace Wezit
{
	public static class ManifestLoader
	{
        #region Fields
        private static string TAG = "<color=red>[WezitManifest]</color>";

		private static Manifest manifest;
        #endregion

        #region Properties
        public static Manifest Manifest { get => manifest; }

		// Settings
		public static string SettingsUrl
		{
			get { return manifest.settings.url; }
		}

		public static string SettingsPath
		{
			get { return manifest.settings.path; }
		}

		// Assets
		public static string AssetsUrl
		{
			get { return manifest.assets.url; }
		}

		public static string AssetsPath
		{
			get { return manifest.assets.path; }
		}

		// Sqlite
		public static string SqliteUrl
		{
			get { return manifest.contents.toursql.url; }
		}

		public static string SqlitePath
		{
			get { return manifest.contents.toursql.path; }
		}

		// Manifest
		public static string ManifestUrl
		{
			get { return manifest.self.url; }
		}

		public static string ManifestPath
		{
			get { return manifest.self.path; }
		}

		// Ids
		public static string InventoryId
		{
			get { return manifest.contents.pid; }
		}

		public static string EntityId
		{
			get { return "00" + manifest.service.entity.id; }
		}


		public static string ApiBaseUrl
		{
			get { return manifest.service.urlbase; }
		}

		public static string ApplicationId
		{
			get { return manifest.pid; }
		}
        #endregion

        #region Methods
        #region Public

        public static async UniTask<bool> Init(string manifestUrl, bool online, bool tryUpdate)
		{
			Debug.Log(TAG + " Init");
#if UNITY_WEBGL
			manifest = await LoadWezitManifest(manifestUrl);
#else
			manifest = await Wezit.FilesDownloader.GetManifest(manifestUrl, online, tryUpdate);
#endif
			string inventoryID = manifest.contents.pid.Split('_')[1];

			return true;
		}
        #endregion

        #region Private
        private static async UniTask<Manifest> LoadWezitManifest(string manifestUrl)
		{
			string manifestJsonString = await Utils.FileUtils.RequestTextContent(manifestUrl, 5);

			if (string.IsNullOrEmpty(manifestJsonString))
			{
				Debug.LogError(TAG + "Can not load manifest file from " + manifestUrl);
				return null;
			}
			else
			{
				Debug.Log(TAG + "Manifest loaded");
				return JsonUtility.FromJson<Manifest>(manifestJsonString);
			}
		}
        #endregion
        #endregion
    }
} // End namespace Wezit
