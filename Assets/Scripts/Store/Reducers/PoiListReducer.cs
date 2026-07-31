using System.Collections.Generic;
using Unidux;

namespace Store
{
	using Pois = List<Wezit.Poi>;

	public static class PoiList
	{
		// Possible types of actions
		public enum ActionType
		{
			Set
		}

		// Actions must have a type and may include a payload
		public class Action
		{
			public ActionType ActionType;
			public object Payload;
		}

		// ActionCreators creates actions and deliver payloads
		public static class ActionCreator
		{
			public static Action Set(Pois inventoryList)
			{
				return new Action() { ActionType = ActionType.Set, Payload = inventoryList };
			}
		}

		// Reducers handle state changes
		public class Reducer : ReducerBase<State, Action>
		{
			public override State Reduce(State state, Action action)
			{
				switch (action.ActionType)
				{
					case ActionType.Set:
						state.PoiList = action.Payload as Pois;
						break;
				}

				return state;
			}
		}
	}
}
