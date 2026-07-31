using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PoiProgressionData
{
    [FormerlySerializedAs("Id")]
    [SerializeField]
    public string Id = "";

    [FormerlySerializedAs("MiniGameCompleted")]
    public bool MiniGameCompleted;

    [FormerlySerializedAs("HasBeenVisited")]
    public bool HasBeenVisited;

    [FormerlySerializedAs("HasCollectedSeed")]
    public bool HasCollectedSeed;

    [FormerlySerializedAs("HasCollectedItem")]
    public bool HasCollectedItem;

    [FormerlySerializedAs("MinigameBestTime")]
    public int MinigameBestTime = 20000;

    #region Public API
    #endregion
}