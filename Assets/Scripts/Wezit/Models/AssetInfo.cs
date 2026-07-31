using System;
using System.IO;

namespace Wezit
{
	[Serializable]
	public class AssetInfo
	{
		public string pid;
		public string label;
		public string path;
		public string url;
		public string mimeType;

		public override string ToString()
		{
			return String.Format(
				"Pid: {0}\n" +
				"Label: {1}\n" +
				"Path: {2}\n" +
				"Url: {3}\n" +
				"MimeType: {4}",
				pid, label, path, url, mimeType
			);
		}

		public string GetSource()
		{
			string fullPath = Path.Combine(DataGrabber.AssetsFolderPath, path);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			fullPath = "file://" + fullPath;
#endif
			return File.Exists(fullPath.Replace("file://", "")) || Directory.Exists(fullPath.Replace("file://", "")) ? fullPath : url;
		}

		public string GetMimeType()
		{
			return mimeType;
		}
	}
}