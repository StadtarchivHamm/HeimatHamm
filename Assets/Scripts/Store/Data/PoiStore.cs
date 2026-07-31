using System.Collections.Generic;
using UnityEngine;

public class PoiStore
{
	public static string TAG = "[PoiStore]";

	public static Wezit.Poi GetPoiById(string poiPid)
	{
		Wezit.Poi poi = StoreAccessor.State.PoiList.Find(x => x.pid == poiPid);

		if (poi == null)
		{
			Debug.LogError(TAG + " GetPoiById : Could not find poi of id " + poiPid);
			return null;
		}

		return poi;
	}

	public static Wezit.Poi GetParentPoiByChildId(string childPid)
	{
		List<Wezit.Poi> poiList = StoreAccessor.State.PoiList;
		foreach (Wezit.Poi poi in poiList)
		{
			if (poi.relationList.Find(relation => relation.pid == childPid) != null)
			{
				return poi;
			}
		}

		return null;
	}
}
