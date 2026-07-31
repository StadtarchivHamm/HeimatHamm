using System;

[Serializable]
public class AppConfigModel
{
	public string versionId;
	public string bundleId;
	public string defaultLanguage;
	public uint standByDelay;
	public string standByDelaySettingKey;
	public string showVersionSettingKey;
	public bool cursorVisible;
	public bool devMode;
	public bool loadWezit;
	public bool loadImages;
	public bool downloadImagesOnStartup;
	public bool downloadOriginalImages;
	public string backgroundColor;
	public float screenFadeTime;
	public bool languageSelecterInteractive;
	public bool languageSelecterVisible;
	public int targetFrameRate = -1;
	public float homeView_diapoShowingTime;
	public float homeView_diapoAnimZoomScale;
	public float homeView_diapoAnimZoomMoveOnX;
	public float homeView_diapoAnimZoomMoveOnY;
	public AppResolutionConfigModel resolutionSettings;

	public override string ToString()
	{
		return String.Format(
			"versionId: {0}\n" +
			"bundleId: {1}\n" +
			"defaultLanguage: {2}\n" +
			"standByDelay: {3}\n" +
			"standByDelaySettingKey: {4}\n" +
			"showVersionSettingKey: {5}\n" +
			"cursorVisible: {6}\n" +
			"devMode: {7}\n" +
			"loadWezit: {8}\n" +
			"loadImages: {9}\n" +
			"downloadImagesOnStartup: {10}\n" +
			"downloadOriginalImages: {11}\n" +
			"backgroundColor: {12}\n" +
			"screenFadeTime: {13}\n" +
			"languageSelecterInteractive: {14}\n" +
			"languageSelecterVisible: {15}\n" +
			"targetFrameRate: {16}\n" +
			"homeView_diapoShowingTime: {17}\n" +
			"homeView_diapoAnimZoomScale: {18}\n" +
			"homeView_diapoAnimZoomMoveOnX: {19}\n" +
			"homeView_diapoAnimZoomMoveOnY: {20}\n" +
			"resolutionSettings: {21}\n",
			versionId,
			bundleId,
			defaultLanguage,
			standByDelay,
			standByDelaySettingKey,
			showVersionSettingKey,
			cursorVisible,
			devMode,
			loadWezit,
			loadImages,
			downloadImagesOnStartup,
			downloadOriginalImages,
			backgroundColor,
			screenFadeTime,
			languageSelecterInteractive,
			languageSelecterVisible,
			targetFrameRate,
			homeView_diapoShowingTime,
			homeView_diapoAnimZoomScale,
			homeView_diapoAnimZoomMoveOnX,
			homeView_diapoAnimZoomMoveOnY,
			resolutionSettings
		);
	}
}
