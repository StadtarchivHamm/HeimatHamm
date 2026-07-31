using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Async;
using UnityEngine;

public class WezitDataUtils
{
	#region Poi
	public static Wezit.Tour GetWezitTourByLang(Language language)
	{
		Wezit.Tour wezitTour = null;
		wezitTour = StoreAccessor.State.TourList.Find((l) => l.language == language.ToString());
		return wezitTour;
    }

    public static List<Wezit.Tour> GetWezitToursByLang(Language language)
    {
        return StoreAccessor.State.TourList.FindAll(x => x.language == language.ToString());
    }

    public static List<Wezit.Tour> GetWezitToursForAllLanguages()
	{
		return StoreAccessor.State.TourList;
	}

	public static Wezit.Poi GetWezitPoiByTag(Language language, string tag)
	{
		Wezit.Poi wzPoiResult = null;
		Wezit.Tour wezitTour = GetWezitTourByLang(language);

		if (wezitTour != null)
		{
			foreach (Wezit.Poi wzPoi in wezitTour.children)
			{
				if (wzPoi.tags == tag)
				{
					wzPoiResult = wzPoi;
					break;
				}
			}
		}

		return wzPoiResult;
	}

	public static Wezit.Poi GetWezitChildPoiByTag(Wezit.Poi wzPoiParent, string tag)
	{
		Wezit.Poi wzPoiResult = null;

		if (wzPoiParent != null)
		{
			foreach (Wezit.Poi wzPoi in wzPoiParent.children)
			{
				if (wzPoi.tags == tag)
				{
					wzPoiResult = wzPoi;
					break;
				}
			}
		}

		return wzPoiResult;
	}

	public static Wezit.Poi GetWezitChildPoiByType(Wezit.Poi wzPoiParent, string type)
	{
		Wezit.Poi wzPoiResult = null;

		if (wzPoiParent != null)
		{
			foreach (Wezit.Poi wzPoi in wzPoiParent.children)
			{
				if (wzPoi.type == type)
				{
					wzPoiResult = wzPoi;
					break;
				}
			}
		}

		return wzPoiResult;
	}

	public static Wezit.Poi GetWezitChildPoiByType(Wezit.Tour wzTourParent, string type)
	{
		Wezit.Poi wzPoiResult = null;

		if (wzTourParent != null)
		{
			foreach (Wezit.Poi wzPoi in wzTourParent.children)
			{
				if (wzPoi.type == type)
				{
					wzPoiResult = wzPoi;
					break;
				}
			}
		}

		return wzPoiResult;
	}

	public static List<Wezit.Poi> GetPoiChildren(Wezit.Poi wzPoiParent)
	{
		if (wzPoiParent != null)
		{
			if (wzPoiParent.children != null && wzPoiParent.children.Count > 0)
			{
				return wzPoiParent.children;
			}
			else
			{
				if (wzPoiParent.relationList != null && wzPoiParent.relationList.Count > 0)
				{
					wzPoiParent.relationList = wzPoiParent.relationList.OrderBy(relation => relation.order).ToList();

					List<Wezit.Poi> childPois = new List<Wezit.Poi>();
					foreach (Wezit.PoiRelation childRelation in wzPoiParent.relationList)
					{
						if (childRelation.relationName == Wezit.RelationName.HAS_NODE)
						{
							childPois.Add(PoiStore.GetPoiById(childRelation.pid));
						}
					}
					wzPoiParent.children = childPois;
					return childPois;
				}
			}
		}
		return null;
	}

	public static async UniTask<Wezit.Poi> GetCorrespondingPoiByLanguage(Wezit.Poi currentPoi, Language language)
	{
		List<Wezit.Poi> resultData = await Wezit.StoreInitializer.GetPoiVersions(currentPoi.pid);

		if (resultData == null)
		{
			Debug.LogError("Could not find corresponding poi of poi : " + currentPoi.pid + "for language : " + language.ToString());
			return null;
		}
		
		Wezit.Poi newPoi = resultData.Find(poi => poi.language == language.ToString());
		if (newPoi == null)
		{
			Debug.LogError("Could not find corresponding poi of poi : " + currentPoi.pid + "for language : " + language.ToString());
			return null;
		}

		return PoiStore.GetPoiById(newPoi.pid);
	}

	public static int ForceMediaListOrderByUsage(Wezit.Relation r1, Wezit.Relation r2)
	{
		return System.Convert.ToInt32(r1.usage.Replace("image/", "")).CompareTo(System.Convert.ToInt32(r2.usage.Replace("image/", "")));
	}

	public static string GetAssetSourceByAssetName(string assetName)
    {
		string result = "";

		return result;
    }
	#endregion
}
