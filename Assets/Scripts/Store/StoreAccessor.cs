using UniRx;
using Unidux;
using UnityEngine;

public sealed class StoreAccessor : Singleton<StoreAccessor>, IStoreAccessor
{
	//public TextAsset InitialStateJson;

	private Store<State> _store;

	public IStoreObject StoreObject
	{
		get { return Store; }
	}

	public static State State
	{
		get { return Store.State; }
	}

	public static Subject<State> Subject
	{
		get { return Store.Subject; }
	}

	private static IReducer[] Reducers
	{
		get
		{
			// Assign reducers
			return new IReducer[] {
				new Store.Kiosk.Reducer(),
				new Store.Manifest.Reducer(),
				new Store.InventoryList.Reducer(),
				new Store.TourList.Reducer(),
				new Store.PoiList.Reducer(),
				new Store.SelectedPoi.Reducer(),
				new Store.LocationList.Reducer(),
				new Store.ThreeDPositionsList.Reducer(),
				new Store.CategoryList.Reducer(),
				new Store.CoverList.Reducer()
			};
		}
	}

	private static State InitialState
	{
		get
		{
			return new State();
		}
	}

	public static Store<State> Store
	{
		get { return Instance._store = Instance._store ?? new Store<State>(InitialState, Reducers); }
	}

	public static object Dispatch<TAction>(TAction action)
	{
		return Store.Dispatch(action);
	}

	void Update()
	{
		Store.Update();
	}
}
