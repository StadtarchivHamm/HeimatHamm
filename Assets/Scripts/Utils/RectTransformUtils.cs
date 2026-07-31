using UnityEngine;

/**
 * Created by Willy J.
 */

namespace Utils
{
	public class RectTransformUtils : MonoBehaviour
	{
		private static Vector3 GetDeltaPositionByPivot(RectTransform rectTransform, Vector2 pivot)
		{
			Vector3 deltaPosition = Vector3.zero;
			if (rectTransform == null) return deltaPosition;

			Vector2 size = rectTransform.rect.size;
			Vector2 deltaPivot = rectTransform.pivot - pivot;
			deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y) * rectTransform.localScale.x;

			return deltaPosition;
		}

		public static Vector3 GetPositionByPivot(RectTransform rectTransform, Vector2 pivot)
		{
			Vector3 resultPoint = Vector3.zero;
			if (rectTransform == null) return resultPoint;

			resultPoint = rectTransform.position - GetDeltaPositionByPivot(rectTransform, pivot);
			return resultPoint;
		}

		public static Vector3 GetLocalPositionByPivot(RectTransform rectTransform, Vector2 pivot)
		{
			Vector3 resultPoint = Vector3.zero;
			if (rectTransform == null) return resultPoint;

			resultPoint = rectTransform.localPosition - GetDeltaPositionByPivot(rectTransform, pivot);
			return resultPoint;
		}

		public static void SetPivot(RectTransform rectTransform, Vector2 pivot)
		{
			if (rectTransform == null || pivot == rectTransform.pivot) return;

			Vector3 deltaPosition = GetDeltaPositionByPivot(rectTransform, pivot);

			rectTransform.pivot = pivot;
			rectTransform.localPosition -= deltaPosition;
		}

		public static float CalculateDistanceBetweenPoints(RectTransform rect, Vector2 pos1, Vector2 pos2)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, pos1, null, out pos1);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, pos2, null, out pos2);
			return Vector2.Distance(pos1, pos2);
		}
	}
}

