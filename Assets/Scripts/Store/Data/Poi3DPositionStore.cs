/**
 * Created by Louis
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Poi3DPositionStore : MonoBehaviour
{
	public static string TAG = "[3DPositionsStore]";

	private static Dictionary<string, Wezit.ThreeDPosition> Poi3DPositionsDict = null;

	public static void Init()
	{
		Poi3DPositionsDict = new Dictionary<string, Wezit.ThreeDPosition>();
		if (StoreAccessor.State.ThreeDLocationList != null)
		{
			foreach (Wezit.ThreeDPosition threeDPosition in StoreAccessor.State.ThreeDLocationList)
            {
				Poi3DPositionsDict.Add(threeDPosition.pid, threeDPosition);
				threeDPosition.InitWezitAsset();
			}
		}
		else
		{
			Debug.LogError("No 3DPositionsList in Store");
		}
	}

	public static Wezit.ThreeDPosition Get3DPositionById(string poi3DPositionPid)
	{
		Wezit.ThreeDPosition output = null;
		if (Poi3DPositionsDict != null && Poi3DPositionsDict.ContainsKey(poi3DPositionPid)) output = Poi3DPositionsDict[poi3DPositionPid];
		return output;
	}

	public static List<Wezit.ThreeDPosition> Get3DPositions()
	{
		return StoreAccessor.State.ThreeDLocationList;
	}

	public static Wezit.ThreeDPosition Get3DPositionByLang(Language language, string a_Pid)
	{
		return Get3DPositions().Find(x => x.pid == a_Pid && x.language == language.ToString());
	}

	public static List<Wezit.ThreeDPosition> Get3DPoisIn3DMap(string mapId)
    {
		List<Wezit.ThreeDPosition> output = new List<Wezit.ThreeDPosition>();
		foreach (Wezit.ThreeDPosition threeDPoi in Get3DPositions())
		{
			if (threeDPoi.map_pid == mapId) output.Add(threeDPoi);
		}
		return output;
	}

	public static List<Wezit.ThreeDPosition> Get3DPoisIn3DMapExceptSome(string mapId, List<Wezit.ThreeDPosition> exceptions)
	{
		List<Wezit.ThreeDPosition> output = new List<Wezit.ThreeDPosition>();
		foreach (Wezit.ThreeDPosition threeDPosition in Get3DPoisIn3DMap(mapId))
		{
			if (!exceptions.Contains(threeDPosition)) output.Add(threeDPosition);
		}
		return output;
	}

	public static List<Wezit.Poi> GetPoisIn3DMapExceptSome(string mapId, List<Wezit.ThreeDPosition> exceptions)
	{
		List<Wezit.Poi> output = new List<Wezit.Poi>();
		foreach (Wezit.ThreeDPosition threeDPosition in Get3DPoisIn3DMap(mapId))
		{
			if (exceptions.Find(x => x.pid == threeDPosition.pid) == null)
            {
				output.Add(PoiStore.GetPoiById(threeDPosition.pid));
            }
		}
		return output;
	}

}
