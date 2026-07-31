using System.Collections.Generic;
using Unidux;

namespace Store
{
	using Locations = List<Wezit.PoiLocation>;

	public static class LocationList
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
			public static Action Set(Locations locationList)
			{
				return new Action() {ActionType = ActionType.Set, Payload = locationList};
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
						state.LocationList = action.Payload as Locations;
						break;
				}

				return state;
			}
		}
	}
}
