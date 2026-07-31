/**
 * Created by Louis
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CategoryStore : MonoBehaviour
{
	public static string TAG = "[CategoryStore]";

	private static Dictionary<string, Wezit.Category> CategoryDict = null;

	public static void Init()
	{
		CategoryDict = new Dictionary<string, Wezit.Category>();
		if (StoreAccessor.State.CategoryList != null)
		{
			foreach (Wezit.Category category in StoreAccessor.State.CategoryList)
				CategoryDict.Add(category.pid, category);
		}
		else
		{
			Debug.LogError("No Category in Store");
		}
	}

	public static List<Wezit.Category> GetCategories()
	{
		return StoreAccessor.State.CategoryList;
	}

	public static List<Wezit.Category> GetCategoriesUnique()
    {
		List<Wezit.Category> output = new List<Wezit.Category>();
		IEnumerable<IGrouping<string, Wezit.Category>> categoriesGroupedById = GetCategories().GroupBy(x => x.categoryId);
		foreach (IGrouping<string, Wezit.Category> group in categoriesGroupedById)
        {
			output.Add(GetCategoryById(group.Key));
        }
		return output;
    }

	public static List<Wezit.Category> GetCategoriesByLanguage(string language)
    {
		return GetCategories().FindAll(x => x.language == language);
    }

	public static Wezit.Category GetCategoryByName(string categoryName)
    {
		Wezit.Category output = null;
		output = GetCategories().Find(x => x.CategoryName == categoryName);

		return output;
	}

	public static Wezit.Category GetCategoryById(string categoryId)
    {
		return GetCategories().Find(x => x.categoryId == categoryId);
	}

	public static Wezit.Category GetPoiCategoryById(string poiPid)
	{
		Wezit.Category output = null;
		if (CategoryDict != null && CategoryDict.ContainsKey(poiPid)) output = CategoryDict[poiPid];
		return output;
	}

	public static List<Wezit.Poi> GetPoisFromCategory(Wezit.Category category)
	{
		List<Wezit.Poi> output = new List<Wezit.Poi>();
		foreach (Wezit.Category cat in GetCategories().FindAll(x => x.categoryId == category.categoryId))
        {
			output.Add(PoiStore.GetPoiById(cat.pid));
        }
		return output;
	}

	public static List<Wezit.Poi> GetPoisFromCategory(string categoryName)
    {
		List<Wezit.Poi> output = GetPoisFromCategory(GetCategoryByName(categoryName));

		return output;
    }
}
