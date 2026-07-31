using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
	public class LayoutGroupRebuilder
	{
		public static IEnumerator Rebuild(GameObject container, float time = 0)
		{
			RectTransform layoutToRebuild = container.GetComponentsInParent<RectTransform>(true)[0];

			if (container && layoutToRebuild)
			{
				while (!(layoutToRebuild.gameObject.activeSelf && layoutToRebuild.gameObject.activeInHierarchy))
                {
					yield return time == 0 ? null : new WaitForSeconds(time);
                }

				LayoutRebuilder.ForceRebuildLayoutImmediate(layoutToRebuild);
			}
		}

		public static async void RebuildAsync(GameObject container)
		{
			RectTransform layoutToRebuild = container.GetComponentsInParent<RectTransform>(true)[0];

			if (container && layoutToRebuild)
			{
				while (!(layoutToRebuild.gameObject.activeSelf && layoutToRebuild.gameObject.activeInHierarchy))
                {
					await Task.Delay(TimeSpan.FromSeconds(0.1f));
				}

				LayoutRebuilder.ForceRebuildLayoutImmediate(layoutToRebuild);
			}

		}

		public static IEnumerator DisableEnable(GameObject container, int loop = 1, float time = 0)
		{
			if (container)
			{
                for (int i = 0; i < loop; i++)
				{
					yield return time == 0 ? null : new WaitForSeconds(time);
					container.SetActive(false);
					yield return time == 0 ? null : new WaitForSeconds(time);
					container.SetActive(true);
                }
			}

		}
	}


}
