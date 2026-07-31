using System.Collections.Generic;
using Unidux;

namespace Store
{
	using Covers = List<Wezit.Cover>;

	public static class CoverList
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
			public static Action Set(Covers coversList)
			{
				return new Action() {ActionType = ActionType.Set, Payload = coversList};
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
						state.CoverList = action.Payload as Covers;
						break;
				}

				return state;
			}
		}
	}
}
