using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public enum EContentProgressionState
{
    Untouched = 0,
    Uncomplete = 1,
    Complete = 2,
}

[Serializable]
public class ContentProgressionData
{
    [FormerlySerializedAs("Id")]
    [SerializeField]
    public string Id = "";

    [FormerlySerializedAs("Progress")]
    [SerializeField]
    public EContentProgressionState State = EContentProgressionState.Untouched;

    #region Public API
    public bool HasBeenVisited
    {
        get
        {
            return State != EContentProgressionState.Untouched;
        }
    }

    public bool HasBeenCompleted
    {
        get
        {
            return State == EContentProgressionState.Complete;
        }
    }

    public void SetCompleted()
    {
        State = EContentProgressionState.Complete;
    }
    #endregion
}