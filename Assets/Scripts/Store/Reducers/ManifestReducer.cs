using Unidux;

namespace Store
{
	public static class Manifest
	{
		// Possible types of actions
		public enum ActionType
		{
			SetManifestPath,
			SetInventoryID
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
			public static Action SetManifestPath(string manifestPath)
			{
				return new Action() { ActionType = ActionType.SetManifestPath, Payload = manifestPath };
			}

			public static Action SetInventoryID(string inventoryID)
			{
				return new Action() { ActionType = ActionType.SetInventoryID, Payload = inventoryID };
			}
		}

		// Reducers handle state changes
		public class Reducer : ReducerBase<State, Action>
		{
			public override State Reduce(State state, Action action)
			{
				switch (action.ActionType)
				{
					case ActionType.SetManifestPath:
						state.ManifestPath = action.Payload.ToString();
						break;

					case ActionType.SetInventoryID:
						state.InventoryID = action.Payload.ToString();
						break;
				}

				return state;
			}
		}
	}
}
