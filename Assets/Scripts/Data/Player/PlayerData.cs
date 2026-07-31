using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;
using Wezit;

[Serializable]
public class PlayerData
{
    #region Saved data
    [FormerlySerializedAs("ToursProgression")]
    [SerializeField]
    public List<TourProgressionData> ToursProgression = new List<TourProgressionData>();

    [FormerlySerializedAs("NumberOfSeeds")]
    [SerializeField]
    public int NumberOfSeeds = 0;

    [FormerlySerializedAs("MaxNumberOfSeeds")]
    [SerializeField]
    public int MaxNumberOfSeeds = 11;

    [FormerlySerializedAs("Language")]
    [SerializeField]
    public string Language = "";

    [FormerlySerializedAs("CheckedARCompatibility")]
    [SerializeField]
    public bool CheckedARCompatibility = false;

    [FormerlySerializedAs("IsARCompatible")]
    [SerializeField]
    public bool IsARCompatible = false;

    [FormerlySerializedAs("HasSeenCompatibilityPopin")]
    [SerializeField]
    public bool HasSeenCompatibilityPopin = false;

    [FormerlySerializedAs("HasSeenTutorial")]
    [SerializeField]
    public bool HasSeenTutorial = false;

    [FormerlySerializedAs("HasSeenSecretPoiUnlockPopin")]
    [SerializeField]
    public bool HasSeenSecretPoiPopin = false;

    [FormerlySerializedAs("UnlockedHiddenObjectsPoiIds")]
    [SerializeField]
    public List<string> UnlockedHiddenObjectsPoiIds = new List<string>();

    [FormerlySerializedAs("ContrastTitleFontSize")]
    [SerializeField]
    public float ContrastTitleFontSize = -1;

    [FormerlySerializedAs("ContrastColorSwapped")]
    [SerializeField]
    public bool ContrastColorSwapped = false;

    [FormerlySerializedAs("ContrastParagraphFontSize")]
    [SerializeField]
    public float ContrastParagraphFontSize = -1;

    [FormerlySerializedAs("UseWheelchairMode")]
    [SerializeField]
    public bool UseWheelchairMode = false;
    #endregion

    #region Public API
    public TourProgressionData GetTourProgression(string a_tourId)
    {
        TourProgressionData progression = ToursProgression.Find(x => x.Id.CompareTo(a_tourId) == 0);
        if(progression == null)
        {
            progression = new TourProgressionData()
            {
                Id = a_tourId,
            };
            ToursProgression.Add(progression);
        }

        return progression;
    }

    public TourProgressionData GetCurrentTourProgression()
    {
        return GetTourProgression(PlayerManager.CurrentState.CurrentTour.pid);
    }

    public PoiProgressionData GetCurrentPoiProgression()
    {
        return GetCurrentTourProgression().GetPoiProgression(PlayerManager.CurrentState.CurrentPoi.pid);
    }

    public PoiProgressionData GetPoiProgression(string a_poiId)
    {
        return GetTourProgression(PlayerManager.CurrentState.CurrentTour.pid).GetPoiProgression(a_poiId);
    }

    public void SetPoiProgression(string a_tourId, string a_poiId)
    {
        if (GetTourProgression(a_tourId).GetPoiProgression(a_poiId).HasBeenVisited) return;
        GetTourProgression(a_tourId).GetPoiProgression(a_poiId).HasBeenVisited = true;
        Save();
    }

    public void SetLanguage(Language language)
    {
        Language = language.ToString();
        Save();
    }

    public void AddUnlockedHiddenObject(string pid)
    {
        if (!UnlockedHiddenObjectsPoiIds.Contains(pid))
        {
            UnlockedHiddenObjectsPoiIds.Add(pid);
            Save();
        }
    }

    public List<Poi> GetUnlockedHiddenObjectsPois()
    {
        List<Poi> bookmarkedPois = new List<Poi>();

        foreach (string pid in UnlockedHiddenObjectsPoiIds)
        {
            Poi poi = PoiStore.GetPoiById(pid);

            if (poi != null)
            {
                bookmarkedPois.Add(poi);
            }
        }

        return bookmarkedPois;
    }
    #endregion

    #region Save/Load behavior
    const string CONST_FILE_NAME = "user.dat";

    #region Fields
    [NonSerialized]
    private string m_fileContent;
    [NonSerialized]
    private string m_filePath;
    [NonSerialized]
    private Thread m_savingThread;
    [NonSerialized]
    private bool m_saving;
    #endregion

    private string FilePath
    {
        get
        {
            return Path.Combine(PlayerManager.PlayerDataPath, CONST_FILE_NAME);
        }
    }

    public void Load(bool a_reset = false)
    {
        if (a_reset)
            return;

        FileStream file;

        if (File.Exists(FilePath))
        {
            file = File.OpenRead(FilePath);
            BinaryFormatter bf = new BinaryFormatter();
            string data = (string) bf.Deserialize(file);
            file.Close();

            JsonUtility.FromJsonOverwrite(data, this);
        }
        else
        {
            Debug.LogWarning("No player data file, creating one...");
        }
    }

    public void Save()
    {
        m_fileContent = JsonUtility.ToJson(this, false);
        m_savingThread = new Thread(SaveData);
        m_filePath = FilePath;
        if(!m_saving) m_savingThread.Start();
    }

    private void SaveData()
    {
        m_saving = true;
        FileStream file;

        if (File.Exists(m_filePath))
        {
            File.WriteAllText(m_filePath, string.Empty);
            file = File.OpenWrite(m_filePath);
        }
        else file = File.Create(m_filePath);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, m_fileContent);
        file.Close();

        m_saving = false;
        m_savingThread.Abort();
    }

    public void Delete()
    {
        if(File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
        ToursProgression.Clear();
        NumberOfSeeds = 0;
        HasSeenTutorial = false;
        UnlockedHiddenObjectsPoiIds.Clear();
        UseWheelchairMode = false;
        MenuManager.Instance.UpdateProgress();
    }
    #endregion
}
