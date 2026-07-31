using System.IO;
using UnityEngine;

public class AppStartUp : Singleton<AppStartUp>
{
    #region Fields
    #endregion

    #region Methods
    #region BeforeSceneLoad
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static async void OnBeforeSceneLoadRuntimeMethod()
	{
		// ORDER IS IMPORTANT
		// Get app config
		await AppConfig.Init();

		// Init Wezit using the app config parameters
		if (AppConfig.ConfigModel.loadWezit)
		{
			// Get Wezit config
			await Wezit.Config.Init();

			Wezit.ConfigModel wezitConfig = Wezit.Config.WezitConfig;
			await Wezit.DataInitializer.Init(wezitConfig.updateWezit,
											 wezitConfig.manifestUrl,
											 wezitConfig.online,
											 wezitConfig.loadLocalAssets,
											 wezitConfig.downloadAssetsOnStartup,
											 wezitConfig.downloadTransformation,
											 wezitConfig.downloadSettingsAssetsOnStartup);
		}



		// Init app
		AppManager.Instance.Init();
	}
    #endregion

    #region Public
    #endregion
    #endregion
}
