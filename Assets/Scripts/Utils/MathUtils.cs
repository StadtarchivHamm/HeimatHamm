/**
 * Created by Willy
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public static class MathUtils
	{
		public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
		{
			return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
		}

		public static float ClampAngle(float angle, float min, float max)
		{
			// accepts e.g. -80, 80
			if (angle < 0f) angle = 360 + angle;
			if (angle > 180f) return Mathf.Max(angle, 360 + min);
			return Mathf.Min(angle, max);
		}

		public static float Deg2Rad(float degAngle)
		{
			return degAngle * Mathf.PI / 180f;
		}

		public static float Rad2Deg(float degAngle)
		{
			return degAngle * 180f / Mathf.PI;
        }

        private static System.Random rng = new System.Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
