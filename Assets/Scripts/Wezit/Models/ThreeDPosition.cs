using System;
using System.Collections.Generic;
namespace Wezit
{
	[Serializable]
	public class ThreeDPosition : Base
	{
		public string tour_pid;
		public string startTime;
		public string endTime;
		public float yaw;
		public float pitch;
		public float roll;
		public float fov;
		public string map_name;
		public string map_pid;
		public WezitAssets.Asset wezitAsset;

		public void InitWezitAsset()
		{
			wezitAsset = AssetsLoader.GetAssetById(map_pid);
		}

		public string GetMapSourceByTransformation(string transformation = "default")
		{
			string source = wezitAsset.GetAssetSourceByTransformation(transformation);

			if (transformation == WezitSourceTransformation.tilesZip && source.Contains("http"))
			{
				source = wezitAsset.GetAssetSourceByTransformation(WezitSourceTransformation.tiles);
			}

			return source;
		}

		public override string ToString()
		{
			return base.ToString() + String.Format(
				"Title: {0}\n" +
				"pid: {1}\n" +
				"map_name: {2}\n" +
				"yaw: {3}\n" +
				"pitch: {4}\n" +
				"roll: {5}\n" +
				"fov: {6}",
				title,
				pid,
				map_name,
				yaw,
				pitch,
				roll,
				fov
			);
		}
	}

}
