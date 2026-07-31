using System.Collections.Generic;
using UnityEngine;

public class PoiLocationStore : MonoBehaviour
{
    public static string TAG = "[PoiLocationStore]";

    private static Dictionary<string, Wezit.PoiLocation> PoiLocationDict = null;

    public static void Init()
    {
        PoiLocationDict = new Dictionary<string, Wezit.PoiLocation>();
        if (StoreAccessor.State.LocationList != null)
        {
            foreach (Wezit.PoiLocation poiLocation in StoreAccessor.State.LocationList)
            {
                if (!PoiLocationDict.ContainsKey(poiLocation.pid))
                {
                    PoiLocationDict.Add(poiLocation.pid, poiLocation);
                    poiLocation.InitWezitAsset();
                }
            }
        }
        else
        {
            Debug.LogError("No LocationList in Store");
        }
    }

    public static Wezit.PoiLocation GetPoiLocationById(string poiLocationPid)
    {
        if (PoiLocationDict == null)
        {
            Init();
        }

        if (!PoiLocationDict.ContainsKey(poiLocationPid))
        {
            Debug.LogWarning("Dict does not contain item with pid " + poiLocationPid);
            return null;
        }

        return PoiLocationDict[poiLocationPid];
    }

}
