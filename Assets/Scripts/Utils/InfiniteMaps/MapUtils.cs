using System;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.Android;

namespace Utils
{
	public static class MapUtils
	{
        #region MapControl
        public static Vector3 CenterMapOnPoints(List<Vector2> points)
		{
			if (points.Count > 0)
			{
				Vector2 minBounds = Mathf.Infinity * Vector2.one;
				Vector2 maxBounds = -Mathf.Infinity * Vector2.one;
				foreach (Vector2 coordinates in points)
				{
					if (coordinates.x < minBounds.x)
					{
						minBounds.x = coordinates.x;
					}
					if (coordinates.y < minBounds.y)
					{
						minBounds.y = coordinates.y;
					}
					if (coordinates.x > maxBounds.x)
					{
						maxBounds.x = coordinates.x;
					}
					if (coordinates.y > maxBounds.y)
					{
						maxBounds.y = coordinates.y;
					}
				}
				return (ZoomToArea(minBounds, maxBounds, 3));
			}
			else return Vector3.zero;
		}

		public static Vector3 ZoomToArea(Vector2 boundsMin, Vector2 boundsMax, float paddingFactor)
		{
			double ry1 = Math.Log((Math.Sin(MathUtils.Deg2Rad(boundsMin.y)) + 1) /
								   Math.Cos(MathUtils.Deg2Rad(boundsMin.y)));
			double ry2 = Math.Log((Math.Sin(MathUtils.Deg2Rad(boundsMax.y)) + 1) /
								   Math.Cos(MathUtils.Deg2Rad(boundsMax.y)));

			double ryc = (ry1 + ry2) / 2f;
			double centerY = MathUtils.Rad2Deg((float)Math.Atan(Math.Sinh(ryc)));

			double resolutionHorizontal = Math.Abs(boundsMax.x - boundsMin.x) / Screen.width;

			double vy0 = Math.Log(Math.Tan(Math.PI * (0.25 + centerY / 360)));
			double vy1 = Math.Log(Math.Tan(Math.PI * (0.25 + boundsMax.y / 360)));
			double viewHeightHalf = Screen.height / 2f;
			double zoomFactorPowered = viewHeightHalf / (40.7436654315252 * (vy1 - vy0));

			double resolutionVertical = 360.0 / (zoomFactorPowered * 256);

			double resolution = Math.Max(resolutionHorizontal, resolutionVertical) * paddingFactor;
			double zoom = Math.Log(360 / (resolution * 256), 2);

			double lon = (boundsMax.x + boundsMin.x) / 2;
			double lat = centerY;

			return(new Vector3((float)lon, (float)lat, (float)zoom));
		}

		const int EARTHRADIUS = 6371000;
		// Calculate distance using Haversine formula
		public static float CalculateDistance(Vector2 pointA, Vector2 pointB)
		{
			float long1 = pointA.y * Mathf.PI / 180;
			float long2 = pointB.y * Mathf.PI / 180;
			float deltaLong = (pointB.y - pointA.y) * Mathf.PI / 180;
			float deltaLat = (pointB.x - pointA.x) * Mathf.PI / 180;

			float a = Mathf.Sin(deltaLong / 2) * Mathf.Sin(deltaLong / 2) +
						Mathf.Cos(long1) * Mathf.Cos(long2) *
						Mathf.Sin(deltaLat / 2) * Mathf.Sin(deltaLat / 2);
			float arcsinOfSqrt = Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
			float distance = 2 * EARTHRADIUS * arcsinOfSqrt;
			return (distance);
		}
        #endregion

        #region LocationService
		public static bool KeepLocating = false;
		public static bool KeepRotating = false;

		private static float _headingVelocity = 0f;
		public static float RotationToNorth;
        public static UnityEvent<Vector2> UserLocationUpdated = new UnityEvent<Vector2>();
		public static Vector2 UserLocation;
		public static float UserRotation;

		private static Coroutine m_rotationCoroutine;
		private static Coroutine m_locationCoroutine;
		private static MonoBehaviour m_locationHost;

		//private static Vector2 m_devLocation = new Vector2(7.815754643432365f, 51.67989423205321f);
		private static Vector2 m_devLocation = new Vector2(7.81033027199944f, 51.6783230736515f);
		//private static Vector2 m_devLocation = new Vector2(7.8097f, 51.677864f);
		//private static Vector2 m_devLocation = new Vector2(7.811236f, 51.679235f);

		public static void UpdateUserLocation(Vector2 userLocation)
		{
			PlayerManager.CurrentState.IsGPSOn = true;

            if (userLocation == PlayerManager.CurrentState.LastKnownPosition)
            {
                return;
            }

            PlayerManager.CurrentState.LastKnownPosition = userLocation;
			PlayerManager.CurrentState.IsUserInTheArea = CalculateDistance(userLocation, m_devLocation) < 10000;
			UserLocation = userLocation;
			Debug.Log("MapUtils: new user location - " + UserLocation);
			try
			{
				UserLocationUpdated?.Invoke(userLocation);
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
			}
			return;
        }


		public static void StartLocationService(MonoBehaviour monoBehaviour, float delay = -1)
		{
			if (delay == -1)
			{
				delay = Wezit.Settings.GetSettingAsFloat("map.settings.location.refreshrate.value", 15);
            }

			// Only skip if the previous run is genuinely still alive. A destroyed host makes
			// m_locationHost == null (Unity's overloaded ==), which is our signal the prior
			// coroutine was killed abnormally and the guard must NOT block a restart.
			if (m_locationCoroutine != null && m_locationHost != null && KeepLocating)
			{
				return;
			}

			// Host alive but deactivated, or a stale reference: stop cleanly before restarting.
			if (m_locationCoroutine != null && m_locationHost != null)
			{
				m_locationHost.StopCoroutine(m_locationCoroutine);
			}

			m_locationHost = monoBehaviour;
			KeepLocating = true;
			m_locationCoroutine = monoBehaviour.StartCoroutine(RunLocationService(delay));
        }

        public static void StopLocationService()
        {
            KeepLocating = false;
            m_locationCoroutine = null;
            m_locationHost = null;

            Input.location.Stop();
        }

        // Wrapper so m_locationCoroutine is always cleared when the service ends, including
        // every early 'yield break' path inside LocationService. (Host destruction bypasses
        // this, but the self-healing guard in StartLocationService covers that case.)
        private static IEnumerator RunLocationService(float delay)
		{
			yield return LocationService(delay);
			m_locationCoroutine = null;
			m_locationHost = null;

            // Stops the location service if there is no need to query location updates continuously.
            Input.location.Stop();
            Input.compass.enabled = false;
        }

		private static IEnumerator LocationService(float delay = 15)
        {
			delay = delay == 0 ? 15 : delay;
			KeepLocating = true;

			Input.compass.enabled = true;

#if UNITY_EDITOR
			bool fakeLocationInEditor = true;
			if (fakeLocationInEditor)
			{
				yield return new WaitForSeconds(2);
				UpdateUserLocation(m_devLocation);
			}
			else
			{
				PlayerManager.CurrentState.IsGPSOn = false;
			}
			yield break;
#endif
			if(PlayerManager.CurrentState.IsInDevMode)
            {
                UpdateUserLocation(m_devLocation);
                yield break;
            }

			if(UserLocation == Vector2.zero)
            {
#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                {
                    Permission.RequestUserPermission(Permission.FineLocation);

                    float permissionTimeout = 10f;
                    while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && permissionTimeout > 0f)
                    {
                        permissionTimeout -= Time.deltaTime;
                        yield return null;
                    }

                    if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                    {
                        PlayerManager.CurrentState.IsGPSOn = false;
                        yield break;
                    }
                }
#elif UNITY_IOS
				Input.location.Start();
				Input.location.Stop();
#endif
            }

			// Check if the user has location service enabled.
			if (!Input.location.isEnabledByUser)
            {
				PlayerManager.CurrentState.LastKnownPosition = Vector2.zero;
				yield break;
            }

			// Starts the location service.
			Input.location.Start(4f, 1f);

			// Waits until the location service initializes
			int maxWait = 20;
			while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
			{
				yield return new WaitForSeconds(1);
				maxWait--;
			}

			// If the service didn't initialize in 20 seconds this cancels location service use.
			if (maxWait < 1)
			{
				Debug.LogWarning("Timed out");
				PlayerManager.CurrentState.IsGPSOn = false;
				yield break;
			}

			// If the connection failed this cancels location service use.
			if (Input.location.status == LocationServiceStatus.Failed)
			{
				Debug.LogError("Unable to determine device location");
				PlayerManager.CurrentState.IsGPSOn = false;
				yield break;
			}

			float timer = delay;
            while (KeepLocating)
            {
				timer += Time.deltaTime;
				if (timer >= delay)
                {
					timer = 0;
					// If the connection succeeded, this retrieves the device's current location and updates the user location accordingly.
					UpdateUserLocation(new Vector2(Input.location.lastData.longitude, Input.location.lastData.latitude));
                }
				yield return null;
            }
		}

		public static void StartRotationService(MonoBehaviour monoBehaviour)
        {
            if (m_rotationCoroutine != null)
            {
				monoBehaviour.StopCoroutine(m_rotationCoroutine);
            }
			m_rotationCoroutine = monoBehaviour.StartCoroutine(RotationService());
        }

		private static IEnumerator RotationService()
		{
			KeepRotating = true;

			Input.compass.enabled = true;

			while (KeepRotating)
			{
				RotationToNorth = Mathf.SmoothDampAngle(RotationToNorth, -Input.compass.trueHeading, ref _headingVelocity, 0.5f);
				yield return null;
			}
			Input.compass.enabled = false;
        }

        #region Place from coordinates
        public static Vector2 GetUnityCoordinatesFromGeoposition(Vector2 centerCoordinates, Vector2 pointCoordinates)
        {
			Vector2 unityCoordinates = Vector2.zero;

			if (centerCoordinates == Vector2.zero || pointCoordinates == Vector2.zero)
			{
				return Vector2.zero;
			}

			return unityCoordinates;
        }
        #endregion
    }
    #endregion
}
