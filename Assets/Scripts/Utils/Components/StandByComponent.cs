using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityEventStandByComponent : UnityEvent { };

public class StandByComponent : MonoBehaviour
{
	/*************************************************************/
	/*********************** PROPERTIES **************************/
	/*************************************************************/

	private Vector3 lastMousePosition;
	public float standByDelay;          // seconds
	public float repeatRate = 0.2f;     // seconds
	private float elapsedTime;          // seconds

	public UnityEventStandByComponent onStandByComplete;
	public UnityEventStandByComponent onStandByReset;

	private bool onStandByCompleteInvoked = false;
	private bool onStandByResetInvoked = false;

	/*************************************************************/
	/******************** INTERNAL METHODS ***********************/
	/*************************************************************/

	void Awake()
	{
		ResetStandBy();
	}

	private void OnEnable()
	{
		Begin();
	}

	private void OnDisable()
	{
		End();
	}

	/*************************************************************/
	/********************** PUBLIC METHODS ***********************/
	/*************************************************************/

	public void Begin()
	{
		InvokeRepeating("OnRepeatRate", 0f, repeatRate);
	}

	public void End()
	{
		CancelInvoke("OnRepeatRate");
	}

	public void ResetStandBy()
	{
		onStandByCompleteInvoked = false;
		onStandByResetInvoked = false;

		lastMousePosition = new Vector3(0, 0, 0);
		elapsedTime = 0;
	}

	/*************************************************************/
	/********************* PRIVATE METHODS ***********************/
	/*************************************************************/

	private void OnRepeatRate()
	{
		CheckMousePosition();

		if (elapsedTime >= standByDelay)
		{
			if (!onStandByCompleteInvoked)
			{
				onStandByResetInvoked = false;
				onStandByCompleteInvoked = true;

				onStandByComplete.Invoke();
			}
		}
	}

	private void CheckMousePosition()
	{
		if (lastMousePosition != Input.mousePosition)
		{
			lastMousePosition = Input.mousePosition;
			elapsedTime = 0;

			if (!onStandByResetInvoked)
			{
				onStandByResetInvoked = true;
				onStandByCompleteInvoked = false;

				onStandByReset.Invoke();
			}
		}
		else
		{
			elapsedTime += repeatRate;
		}
	}
}
