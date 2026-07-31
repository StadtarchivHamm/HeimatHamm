/**
 * Created by Willy
 */

using System.Collections;
using UnityEngine;

public class FadeEffect : MonoBehaviour
{
	public CanvasGroup canvasGroup;
	public AnimationCurve animationCurve;

	public enum Direction { FadeIn, FadeOut };

	void Start()
	{
		if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null) Debug.LogError("No CanvasGroup in the gameObject");

		if (animationCurve.length == 0)
		{
			Debug.Log("Animation curve not assigned: Create a default animation curve");
			animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}
	}

	public IEnumerator FadeCanvas(Direction direction, float duration)
	{
		float remainAlpha = animationCurve.Evaluate((direction == Direction.FadeIn) ? 1f : 0f) - canvasGroup.alpha;
		float remainDuration = duration * remainAlpha;

		var startTime = Time.time;
		var endTime = Time.time + remainDuration;
		var elapsedTime = 0f;

		if (remainDuration > 0)
		{
			while (Time.time <= endTime)
			{
				elapsedTime = Time.time - startTime;
				var percentage = 1 / (remainDuration / elapsedTime);
				if ((direction == Direction.FadeOut))
				{
					canvasGroup.alpha = animationCurve.Evaluate(1f - percentage);
				}
				else
				{
					canvasGroup.alpha = animationCurve.Evaluate(percentage);
				}

				yield return new WaitForEndOfFrame();
			}
		}

		// force the alpha to the end alpha before finishing – this is here to mitigate any rounding errors, e.g. leaving the alpha at 0.01 instead of 0
		if (direction == Direction.FadeIn) canvasGroup.alpha = animationCurve.Evaluate(1f);
		else canvasGroup.alpha = animationCurve.Evaluate(0f);
	}
}