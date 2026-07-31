using System;
using System.Collections.Generic;
using Unidux;
using Wezit;

[Serializable]
public class State : StateBase
{
	public KioskState KioskState = KioskState.NONE;
	public Language Language = Language.none;
	public string ManifestPath = "";
	public string InventoryID = "";
	public List<Inventory> InventoryList = new List<Inventory>();
	public List<Tour> TourList = new List<Tour>();
	public List<PoiLocation> LocationList = new List<PoiLocation>();
	public List<ThreeDPosition> ThreeDLocationList = new List<ThreeDPosition>();
	public List<Poi> PoiList = new List<Poi>();
	public List<Category> CategoryList = new List<Category>();
	public List<Cover> CoverList = new List<Cover>();

	public Poi SelectedPoi = null;
}
