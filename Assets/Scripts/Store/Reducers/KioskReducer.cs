using Unidux;

namespace Store
{
	public static class Kiosk
	{
		// Possible types of actions
		public enum ActionType
		{
			SetState,
			SetLanguage
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
			public static Action SetState(KioskState state)
			{
				return new Action() { ActionType = ActionType.SetState, Payload = state };
			}

			public static Action SetLanguage(Language lang)
			{
				return new Action() { ActionType = ActionType.SetLanguage, Payload = lang };
			}
		}

		// Reducers handle state changes
		public class Reducer : ReducerBase<State, Action>
		{
			public override State Reduce(State state, Action action)
			{
				switch (action.ActionType)
				{
					case ActionType.SetState:
						state.KioskState = (KioskState)action.Payload;
						break;

					case ActionType.SetLanguage:
						state.Language = (Language)action.Payload;
						break;
				}

				return state;
			}
		}
	}
}
