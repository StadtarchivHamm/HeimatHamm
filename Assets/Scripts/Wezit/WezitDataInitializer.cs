using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Wezit
{
	public class DataInitializer
	{
		public static async Task Init(bool updateWezit, string manifestUrl, bool online, bool loadAssets, bool downloadAssetsOnStartup, string downloadTransformation, bool downloadSettingsAssetsOnStartup)
		{
			await ManifestLoader.Init(manifestUrl, online, updateWezit);
#if !UNITY_WEBGL || UNITY_EDITOR
			if (!online)
			{
				await FilesDownloader.GetSqlite(updateWezit);
			}
#endif
			await Settings.Init(online);
			await AssetsLoader.Init(online);
			await StoreInitializer.Init();
#if !UNITY_WEBGL || UNITY_EDITOR
			if (loadAssets && !online)
			{
				DataGrabber.Instance.Load();
				DataGrabber.Instance.AppDefaultTransformation = downloadTransformation;
				if (downloadAssetsOnStartup)
				{
					await DataGrabber.Instance.GetAllAssets(downloadTransformation);
				}
				else if (downloadSettingsAssetsOnStartup)
				{
					await DataGrabber.Instance.GetSettingsAssets(downloadTransformation);
				}
			}
#endif
		}
	}
}