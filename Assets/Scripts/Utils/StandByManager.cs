using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StandByManager : Singleton<StandByManager>
{
    #region Fields
    private static string TAG = "[StandByManager]";

	private Vector3 lastMousePosition;
	private float standByDelay;         // seconds
	private float repeatRate;           // seconds
	private float elapsedTime;          // seconds

	private Stack<bool> blockerStack;
	private bool blockerFlag;
    #endregion

    #region Properties
    public UnityEvent OnStandByComplete = new UnityEvent();
	public UnityEvent OnStandByReset = new UnityEvent();
    #endregion

    #region Methods
    #region Public
	public void Init(float a_delay, float a_repeatRate = 1f)
	{
		standByDelay = a_delay;
		repeatRate = a_repeatRate;

		lastMousePosition = new Vector3(0, 0, 0);
		elapsedTime = 0;
	}

	public void Begin()
	{
		Debug.Log(TAG + " - starting StandBy with a repeatRate of " + repeatRate);
		InvokeRepeating("OnRepeatRate", 0f, repeatRate);
	}

	public void End()
	{
		Debug.Log(TAG + " - ending StandBy");
		CancelInvoke("OnRepeatRate");
	}

	public void AddBlockerToStack()
	{
		if (blockerStack != null)
		{
			if ((blockerStack.Count == 0) && (blockerFlag == false))
			{
				blockerFlag = true;
			}

			blockerStack.Push(true);
		}
	}

	public void RemoveBlockerToStack()
	{
		if (blockerStack != null && blockerStack.Count > 0)
		{
			blockerStack.Pop();

			if ((blockerStack.Count == 0) && (blockerFlag == true))
			{
				blockerFlag = false;
				elapsedTime = 0;
			}
		}
	}
    #endregion

    #region Private
	private void OnRepeatRate()
	{
		CheckMousePosition();

		if ((elapsedTime >= standByDelay) && !blockerFlag)
		{
			OnStandByComplete.Invoke();
		}
	}

	private void CheckMousePosition()
	{
		if ((lastMousePosition != Input.mousePosition) && !blockerFlag)
		{
			lastMousePosition = Input.mousePosition;
			ResetStandBy();
		}
		else
		{
			elapsedTime += repeatRate;
		}
	}

	private void ResetStandBy()
	{
		elapsedTime = 0;
		OnStandByReset.Invoke();
	}
    #endregion
    #endregion
}
