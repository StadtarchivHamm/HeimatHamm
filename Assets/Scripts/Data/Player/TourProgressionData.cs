using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class TourProgressionData
{
    [FormerlySerializedAs("Id")]
    [SerializeField]
    public string Id = "";

    [FormerlySerializedAs("PoisProgression")]
    [SerializeField] public List<PoiProgressionData> PoisProgression = new List<PoiProgressionData>();

    [FormerlySerializedAs("HasBeenDownloaded")]
    [SerializeField] public bool HasBeenDownloaded = false;

    [FormerlySerializedAs("PercentOfCompletion")]
    [SerializeField] public float PercentOfCompletion = 0;

    #region Public API
    public PoiProgressionData GetPoiProgression(string a_poiId)
    {
        PoiProgressionData progression = PoisProgression.Find(x => x.Id.CompareTo(a_poiId) == 0);
        if (progression == null)
        {
            progression = new PoiProgressionData()
            {
                Id = a_poiId,
            };
            PoisProgression.Add(progression);
        }

        return progression;
    }
    #endregion

}
