using System;

[Serializable]
public class AppResolutionConfigModel
{
	public bool force;
	public int targetWidth;
	public int targetHeight;
	public bool fullscreen;
	public bool checkChanges;

	public override string ToString()
	{
		return String.Format(
			"force: {0}\n" +
			"targetWidth: {1}\n" +
			"targetHeight: {2}\n" +
			"fullscreen: {3}\n" +
			"checkChanges: {4}\n",
			force,
			targetWidth,
			targetHeight,
			fullscreen,
			checkChanges
		);
	}
}
