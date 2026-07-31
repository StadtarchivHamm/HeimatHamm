using System;

namespace Wezit
{
	[Serializable]
	public class ConfigModel
	{
		public bool updateWezit;
		public string manifestUrl;
		public string version;
		public bool online;
		public bool loadLocalAssets;
		public bool downloadAssetsOnStartup;
		public string downloadTransformation;
		public bool downloadSettingsAssetsOnStartup;

		public override string ToString()
		{
			return string.Format(
				"updateWezit: {0}\n" +
				"manifestUrl: {1}\n" +
				"version: {2}\n" +
				"online: {3}\n" +
				"loadImages: {4}\n" +
				"downloadAssetsOnStartup: {5}\n" +
				"downloadTransformation: {6}" +
				"downloadSettingsAssetsOnStartup: {7}",
				updateWezit,
				manifestUrl,
				version,
				online,
				loadLocalAssets,
				downloadAssetsOnStartup,
				downloadTransformation,
				downloadSettingsAssetsOnStartup
			);
		}
	}
}
