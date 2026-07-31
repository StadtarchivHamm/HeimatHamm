using System.Collections.Generic;
using Unidux;

namespace Store
{
	using Inventories = List<Wezit.Inventory>;

	public static class InventoryList
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
			public static Action Set(Inventories inventoryList)
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
						state.InventoryList = action.Payload as Inventories;
						break;
				}

				return state;
			}
		}
	}
}
