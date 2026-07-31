using System.Collections.Generic;
using UnityEngine;

public class CoverStore : MonoBehaviour
{
	public static string TAG = "[PoiStore]";

	public static void Init()
	{
	}

	public static Wezit.Cover GetCoverForPid(string pid_src)
	{
		List<Wezit.Cover> coverList = StoreAccessor.State.CoverList;
		Wezit.Cover cover = StoreAccessor.State.CoverList.Find(x => x.pid_src == pid_src);
		return cover;
	}

	public static Wezit.WezitAssets.Asset GetCoverAssetForPid(string pid_src)
	{
		Wezit.WezitAssets.Asset asset = null;
		Wezit.Cover cover = GetCoverForPid(pid_src);
		if (cover != null)
		{
			asset = Wezit.AssetsLoader.GetAssetById(cover.pid_dest);
		}
		return asset;
	}

	public static string GetCoverSourceByTransformationForPid(string pid_src, string transformation = "original")
	{
		string source = "";
		Wezit.WezitAssets.Asset asset = GetCoverAssetForPid(pid_src);
		if(asset != null)
        {
			source = asset.GetAssetSourceByTransformation(transformation);
        }

		return source;
	}
}
