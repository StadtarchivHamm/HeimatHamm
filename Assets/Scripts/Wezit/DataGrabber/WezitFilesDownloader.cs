using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Wezit
{
	public class FilesDownloader
	{
		public static bool SqliteUpdated
		{
			get { return m_SQliteUpdateNeeded; }
			set { m_SQliteUpdateNeeded = value; }
		}

		#region PathAccessors
		// Sqlite
		public static string SqliteFullPath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit", ManifestLoader.SqlitePath); }
		}

		public static string SqliteEtagPath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit", "sqlite_ETag.txt"); }
		}

		// Manifest
		public static string ManifestFullPath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit", "manifest.json"); }
		}

		public static string ManifestEtagPath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit", "manifest_ETag.txt"); }
		}

		// Settings
		public static string SettingsFullPath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit", ManifestLoader.SettingsPath); }
		}

		public static string SettingsEtagPath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit", "manifest_ETag.txt"); }
		}

		// Assets
		public static string AssetsFullPath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit", ManifestLoader.AssetsPath); }
		}

		public static string AssetsEtagPath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit", "manifest_ETag.txt"); }
		}
		#endregion

		#region Sqlite
		public static async UniTask GetSqlite(bool tryUpdate)
		{
			if (!File.Exists(SqliteFullPath))
			{
				Debug.LogWarning("The database does not exist, downloading...");
				await DownloadSqlite();
				SqliteUpdated = false;
			}
			else
			{
				if (tryUpdate)
				{
					bool updateNeeded = await CheckSqliteUpdateNeeded();
					if (updateNeeded)
					{
						Debug.LogWarning("A database update is needed, downloading...");
						await DownloadSqlite();
						SqliteUpdated = true;
					}
					else
					{
						Debug.Log("Database exists");
						SqliteUpdated = false;
					}
				}
				else
				{
					SqliteUpdated = false;
				}
			}
		}

		public static async UniTask<bool> DownloadSqlite()
		{
			string uri = ManifestLoader.SqliteUrl;

			using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
			{
				webRequest.timeout = 30;
				// Request and wait for the desired page.
				await webRequest.SendWebRequest();
				bool success = false;
				string[] pages = uri.Split('/');
				int page = pages.Length - 1;

				switch (webRequest.result)
				{
					case UnityWebRequest.Result.ConnectionError:
					case UnityWebRequest.Result.DataProcessingError:
						Debug.LogError(pages[page] + ": Error: " + webRequest.error);
						success = false;
						break;
					case UnityWebRequest.Result.ProtocolError:
						success = false;
						Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
						break;
					case UnityWebRequest.Result.Success:
						success = true;
						try
						{
							if (!Directory.Exists(Path.GetDirectoryName(SqliteFullPath)))
							{
								Directory.CreateDirectory(Path.GetDirectoryName(SqliteFullPath));
							}

							File.WriteAllBytes(SqliteFullPath, webRequest.downloadHandler.data);
						}
						catch (Exception e)
						{
							Debug.LogError(e);
						}
						File.WriteAllText(SqliteEtagPath, webRequest.GetResponseHeader("ETag"));
						Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
						break;
				}
				Debug.LogWarning("Download was a " + (success ? "success" : "failure"));
				return success;
			}
		}

		private static bool m_SQliteUpdateNeeded;

		public static async UniTask<bool> CheckSqliteUpdateNeeded()
		{
			if (UnityEngine.Application.internetReachability != NetworkReachability.NotReachable)
			{
				// Get remote ETag
				UnityWebRequest head = UnityWebRequest.Head(ManifestLoader.SqliteUrl);
				head.timeout = 30;

				try
				{
					await head.SendWebRequest();
					if (head.result != UnityWebRequest.Result.Success)
					{
						return false;
					}
					string remoteETag = head.GetResponseHeader("ETag");

					// Get local version
					if (!File.Exists(SqliteEtagPath)) return true;
					string localETag = File.ReadAllText(SqliteEtagPath);

					bool updateNeeded = localETag != remoteETag;
					m_SQliteUpdateNeeded = updateNeeded;
					return updateNeeded;
				}
				catch (Exception error)
				{
					Debug.LogError(error);
					return false;
				}
			}
			else
			{
				m_SQliteUpdateNeeded = false;
				return false;
			}
		}
		#endregion

		#region Assets
		public static async UniTask<string> GetAssets(bool online)
		{
			string assetsPath = Wezit.ManifestLoader.AssetsUrl;

#if !UNITY_WEBGL
			if (!online)
			{
				if (!File.Exists(AssetsFullPath) || m_SQliteUpdateNeeded)
				{
					Debug.LogWarning("Assets don't exist or need an update, downloading");
					bool downloadSuccess = online ? await DownloadAssets() : await DownloadAssets();
					if (downloadSuccess) assetsPath = AssetsFullPath;
				}
				else
				{
					Debug.Log("Assets exist");
					assetsPath = AssetsFullPath;
				}
			}
#endif
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			assetsPath = "file://" + assetsPath;
#endif
			string assetsJsonString = await Utils.FileUtils.RequestTextContent(assetsPath, 5);

			if (string.IsNullOrEmpty(assetsJsonString))
			{
				Debug.LogError("Can not load assets from " + assetsPath);
				return null;
			}
			else
			{
				Debug.Log("Assets loaded \n");
				return assetsJsonString;
			}
		}

		private static async UniTask<bool> DownloadAssets()
		{
			string assetsPath = Wezit.ManifestLoader.AssetsUrl;

			using (UnityWebRequest webRequest = UnityWebRequest.Get(assetsPath))
			{
				webRequest.timeout = 30;
				// Request and wait for the desired page.
				await webRequest.SendWebRequest();
				bool success = false;
				string[] pages = assetsPath.Split('/');
				int page = pages.Length - 1;

				switch (webRequest.result)
				{
					case UnityWebRequest.Result.ConnectionError:
					case UnityWebRequest.Result.DataProcessingError:
						Debug.LogError(assetsPath + ": Error: " + webRequest.error);
						success = false;
						break;
					case UnityWebRequest.Result.ProtocolError:
						success = false;
						Debug.LogError(assetsPath + ": HTTP Error: " + webRequest.error);
						break;
					case UnityWebRequest.Result.Success:
						success = true;
						if (!Directory.Exists(Path.GetDirectoryName(AssetsFullPath)))
						{
							Directory.CreateDirectory(Path.GetDirectoryName(AssetsFullPath));
						}
						File.WriteAllText(AssetsFullPath, webRequest.downloadHandler.text);
						break;
				}
				Debug.LogWarning("Download was a " + (success ? "success" : "failure"));
				return success;
			}
		}

		#endregion

		#region Manifest
		public static async UniTask<Wezit.Manifest> GetManifest(string manifestUrl, bool online, bool tryUpdate)
		{
#if !UNITY_WEBGL
			if (!online)
			{
				if (!File.Exists(ManifestFullPath))
				{
					Debug.LogWarning("The manifest doesn't exist or needs an update, downloading");
					bool downloadSuccess = online ? await DownloadManifest(manifestUrl) : await DownloadManifest(manifestUrl);
					if (downloadSuccess) manifestUrl = ManifestFullPath;
				}
				else
				{
					if (tryUpdate)
					{
						bool updateNeeded = await CheckManifestUpdateNeeded(manifestUrl);
						if (updateNeeded)
						{
							Debug.LogWarning("A manifest update is needed, downloading...");
							await DownloadManifest(manifestUrl);
						}
						else
						{
							Debug.Log("Manifest exists");
						}
					}

					manifestUrl = ManifestFullPath;
				}
			}
#endif

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		manifestUrl = "file://" + manifestUrl;
#endif
			string manifestJsonString = await Utils.FileUtils.RequestTextContent(manifestUrl, 5);

			if (string.IsNullOrEmpty(manifestJsonString))
			{
				Debug.LogError("Can not load manifest file from " + manifestUrl);
				return null;
			}
			else
			{
				Debug.Log("Manifest loaded");
				return JsonUtility.FromJson<Wezit.Manifest>(manifestJsonString);
			}
		}

		private static async UniTask<bool> DownloadManifest(string manifestUrl)
		{
			using (UnityWebRequest webRequest = UnityWebRequest.Get(manifestUrl))
			{
				webRequest.timeout = 30;
				// Request and wait for the desired page.
				await webRequest.SendWebRequest();
				bool success = false;
				string[] pages = manifestUrl.Split('/');
				int page = pages.Length - 1;

				switch (webRequest.result)
				{
					case UnityWebRequest.Result.ConnectionError:
					case UnityWebRequest.Result.DataProcessingError:
						Debug.LogError(manifestUrl + ": Error: " + webRequest.error);
						success = false;
						break;
					case UnityWebRequest.Result.ProtocolError:
						success = false;
						Debug.LogError(manifestUrl + ": HTTP Error: " + webRequest.error);
						break;
					case UnityWebRequest.Result.Success:
						success = true;
						if (!Directory.Exists(Path.GetDirectoryName(ManifestFullPath)))
						{
							Directory.CreateDirectory(Path.GetDirectoryName(ManifestFullPath));
						}
						File.WriteAllText(ManifestFullPath, webRequest.downloadHandler.text);
						File.WriteAllText(ManifestEtagPath, webRequest.GetResponseHeader("ETag"));
						break;
				}
				Debug.LogWarning("Download was a " + (success ? "success" : "failure"));
				return success;
			}
		}

		public static async UniTask<bool> CheckManifestUpdateNeeded(string manifestUrl)
		{
			if (UnityEngine.Application.internetReachability != NetworkReachability.NotReachable)
			{
				// Get remote ETag
				UnityWebRequest head = UnityWebRequest.Head(manifestUrl);
				head.timeout = 30;
				try
				{
					await head.SendWebRequest();
					if (head.result != UnityWebRequest.Result.Success)
					{
						return false;
					}
					string remoteETag = head.GetResponseHeader("ETag");

					// Get local version
					if (!File.Exists(ManifestEtagPath)) return true;
					string localETag = File.ReadAllText(ManifestEtagPath);

					bool updateNeeded = localETag != remoteETag;
					return updateNeeded;
				}
				catch (Exception error)
				{
					Debug.LogError(error);
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region Settings
		public static async UniTask<JSONNode> GetSettings(bool online)
		{
			string settingsPath = Wezit.ManifestLoader.SettingsUrl;

#if !UNITY_WEBGL
			if (!online)
			{
				if (!File.Exists(SettingsFullPath) || m_SQliteUpdateNeeded)
				{
					Debug.LogWarning("Settings don't exist or need an update, downloading");
					bool downloadSuccess = online ? await DownloadSettings() : await DownloadSettings();
					if (downloadSuccess) settingsPath = SettingsFullPath;
				}
				else
				{
					Debug.Log("Settings exist");
					settingsPath = SettingsFullPath;
				}
			}
#endif
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		settingsPath = "file://" + settingsPath;
#endif
			string settingsJsonString = await Utils.FileUtils.RequestTextContent(settingsPath, 5);

			if (string.IsNullOrEmpty(settingsJsonString))
			{
				Debug.LogError("Can not load settings from " + settingsPath);
				return null;
			}
			else
			{
				Debug.Log("Settings loaded");
				return JSON.Parse(settingsJsonString);
			}
		}

		private static async UniTask<bool> DownloadSettings()
		{
			string settingsPath = Wezit.ManifestLoader.SettingsUrl;

			using (UnityWebRequest webRequest = UnityWebRequest.Get(settingsPath))
			{
				webRequest.timeout = 30;
				// Request and wait for the desired page.
				await webRequest.SendWebRequest();
				bool success = false;
				string[] pages = settingsPath.Split('/');
				int page = pages.Length - 1;

				switch (webRequest.result)
				{
					case UnityWebRequest.Result.ConnectionError:
					case UnityWebRequest.Result.DataProcessingError:
						Debug.LogError(settingsPath + ": Error: " + webRequest.error);
						success = false;
						break;
					case UnityWebRequest.Result.ProtocolError:
						success = false;
						Debug.LogError(settingsPath + ": HTTP Error: " + webRequest.error);
						break;
					case UnityWebRequest.Result.Success:
						success = true;
						if (!Directory.Exists(Path.GetDirectoryName(SettingsFullPath)))
						{
							Directory.CreateDirectory(Path.GetDirectoryName(SettingsFullPath));
						}
						File.WriteAllText(SettingsFullPath, webRequest.downloadHandler.text);
						break;
				}
				Debug.LogWarning("Download was a " + (success ? "success" : "failure"));
				return success;
			}
		}

		#endregion
	}
}