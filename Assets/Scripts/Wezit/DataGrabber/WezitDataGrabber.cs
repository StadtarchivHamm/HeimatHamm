using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Wezit
{
    public class DataGrabber : Singleton<DataGrabber>
    {
        #region Saved data
        [Serializable]
        public class AssetAndMd5
        {
            [FormerlySerializedAs("pid")]
            [SerializeField]
            public string pid;

            [FormerlySerializedAs("path")]
            [SerializeField]
            public string path;

            [FormerlySerializedAs("md5")]
            [SerializeField]
            public string md5;

            public AssetAndMd5(string a_pid, string a_path, string a_md5)
            {
                pid = a_pid;
                path = a_path;
                md5 = a_md5;
            }
        }

        [FormerlySerializedAs("DownloadedAssetsMD5Dict")]
        [SerializeField]
        public List<AssetAndMd5> DownloadedAssetsMd5Dict = new List<AssetAndMd5>();

        [FormerlySerializedAs("HasDownloaded")]
        [SerializeField]
        public bool HasDownloaded = false;
        #endregion

        #region Fields
        private string m_fileContent;
        private string m_filePath;
        private Thread m_savingThread;
        private bool m_saving;
        private bool m_loaded;

        private bool m_stopDownload = false;
        private List<WezitAssets.Asset> m_assets = new List<WezitAssets.Asset>();

        public static string AssetsFolderPath { get { return Path.Combine(UnityEngine.Application.persistentDataPath, "wezit"); } }
        public List<WezitAssets.Asset> Assets { get { return m_assets; } }
        public string AppDefaultTransformation;
        #endregion

        #region Properties
        public UnityEvent<int, string> DownloadProgress = new UnityEvent<int, string>();
        public UnityEvent DownloadOver = new UnityEvent();
        #endregion

        #region Methods
        public List<AssetAndMd5> GetAssetsAndMd5Dict()
        {
            Load();
            return DownloadedAssetsMd5Dict;
        }

        public async UniTask GetAllAssets(string transformation = "")
        {
            m_stopDownload = false;
            Load();
            if (string.IsNullOrEmpty(transformation))
            {
                transformation = AppDefaultTransformation;
            }

            if (m_assets.Count == 0)
            {
                if (AssetsLoader.Assets.Count == 0)
                {
                    await AssetsLoader.Init(true);
                }
                else
                {
                    m_assets = AssetsLoader.Assets;
                }
            }

            int currentlyDownloaded = 0;
            int currentCounter = 0;
            for (int i = 0; i < m_assets.Count; i++)
            {
                if (m_stopDownload) return;
                (currentlyDownloaded, currentCounter) = await DownloadAsset(m_assets[i], currentlyDownloaded, currentCounter, m_assets[(i + 1) % m_assets.Count].title, transformation);
            }
            HasDownloaded = true;
            Save();
            Wezit.FilesDownloader.SqliteUpdated = false;
            DownloadOver?.Invoke();
        }

        public async UniTask GetAssetsForTour(string tourId, string transformation = "")
        {
            m_stopDownload = false;
            Load();

            if (string.IsNullOrEmpty(transformation))
            {
                transformation = AppDefaultTransformation;
            }
            int currentlyDownloaded = 0;
            int currentCounter = 0;
            List<WezitAssets.Asset> tourAssets = AssetsLoader.GetAssetsForTour(tourId);
            for (int i = 0; i < tourAssets.Count; i++)
            {
                if (m_stopDownload) return;
                (currentlyDownloaded, currentCounter) = await DownloadAsset(tourAssets[i], currentlyDownloaded, currentCounter, tourAssets[(i + 1) % tourAssets.Count].title, transformation);
            }
            HasDownloaded = true;
            Save();
            FilesDownloader.SqliteUpdated = false;
            DownloadOver?.Invoke();
        }

        public async UniTask GetSettingsAssets(string transformation = "")
        {
            m_stopDownload = false;
            Load();

            if (string.IsNullOrEmpty(transformation))
            {
                transformation = AppDefaultTransformation;
            }
            int currentlyDownloaded = 0;
            int currentCounter = 0;
            List<WezitAssets.Asset> settingsAssets = AssetsLoader.GetAllSettingsAssets();
            for (int i = 0; i < settingsAssets?.Count; i++)
            {
                if (m_stopDownload) return;
                (currentlyDownloaded, currentCounter) = await DownloadAsset(settingsAssets[i], currentlyDownloaded, currentCounter, settingsAssets[(i + 1) % settingsAssets.Count].title, transformation);
            }
            HasDownloaded = true;
            Save();
            Wezit.FilesDownloader.SqliteUpdated = false;
            DownloadOver?.Invoke();
        }

        // Download size
        public int GetDownloadSizeForAssets(List<WezitAssets.Asset> assets, string transformation = "")
        {
            if (assets == null)
            {
                return 0;
            }

            int downloadSize = 0;
            if (string.IsNullOrEmpty(transformation))
            {
                transformation = AppDefaultTransformation;
            }

            foreach (WezitAssets.Asset asset in assets)
            {
                string assetTransformation = asset.usages.Contains("maps") ? "tiles-zip" : transformation;
                bool hasTransformation = false;

                foreach (WezitAssets.File file in asset.files)
                {
                    if (file.label == transformation || transformation == "all")
                    {
                        downloadSize += file.size;
                        hasTransformation = true;
                    }
                }

                if (!hasTransformation)
                {
                    WezitAssets.File file = asset.files.Find(x => x.label == "original");

                    if (file != null)
                    {
                        downloadSize += file.size;
                    }
                }
            }
            return downloadSize;
        }

        public int GetDownloadSize(string transformation = "all")
        {
            return GetDownloadSizeForAssets(m_assets, transformation);
        }

        public int GetDownloadSizeForTour(string tourId, string transformation = "all")
        {
            if (string.IsNullOrEmpty(transformation))
            {
                transformation = AppDefaultTransformation;
            }

            List<WezitAssets.Asset> tourAssets = AssetsLoader.GetAssetsForTour(tourId);
            return GetDownloadSizeForAssets(tourAssets, transformation);
        }

        // Update size
        public int GetUpdateSizeForAssets(List<WezitAssets.Asset> assets, string transformation = "")
        {
            Load();
            if (string.IsNullOrEmpty(transformation))
            {
                transformation = AppDefaultTransformation;
            }

            assets = assets == null ? m_assets : assets;

            List<WezitAssets.File> filesToUpdate = new List<WezitAssets.File>();
            foreach (WezitAssets.Asset asset in assets)
            {
                string assetTransformation = asset.usages.Contains("maps") ? "tiles-zip" : transformation;

                bool hasTransformation = false;
                foreach (WezitAssets.File file in asset.files)
                {
                    if (file.label == assetTransformation || assetTransformation == "all")
                    {
                        hasTransformation = true;
                        if (CheckDownloadNecessity(file))
                        {
                            Debug.Log("There is a file to update or download \nAsset name: " + asset.title + "\nFile uri: " + file.uri);
                            filesToUpdate.Add(file);
                        }
                    }
                }

                if (!hasTransformation)
                {
                    WezitAssets.File file = asset.files.Find(x => x.label == "original");

                    if (file != null)
                    {
                        if (CheckDownloadNecessity(file))
                        {
                            Debug.Log("There is a file to update or download \nAsset name: " + asset.title + "\nFile uri: " + file.uri);
                            filesToUpdate.Add(file);
                        }
                    }
                }
            }

            int downloadSize = filesToUpdate.Sum(x => x.size);
            return downloadSize;
        }

        public int GetUpdateSizeForTour(string tourId, string transformation = "")
        {
            List<WezitAssets.Asset> tourAssets = AssetsLoader.GetAssetsForTour(tourId);
            if (string.IsNullOrEmpty(transformation))
            {
                transformation = AppDefaultTransformation;
            }

            return GetUpdateSizeForAssets(tourAssets, transformation);
        }

        public int GetUpdateSize(string transformation = "")
        {
            return GetUpdateSizeForAssets(null, transformation);
        }

        public bool CheckDownloadNecessity(WezitAssets.File file)
        {
            return CheckDownloadNecessity(Path.Combine(AssetsFolderPath, file.path), file.md5);
        }

        public bool CheckDownloadNecessity(string path, string md5)
        {
            AssetAndMd5 assetMd5 = DownloadedAssetsMd5Dict.Find(x => x.path == path);
            if (assetMd5 != null)
            {
                return assetMd5.md5 != md5;
            }
            else
            {
                bool fileExists = File.Exists(path) || Directory.Exists(path);
                return !fileExists;
            }
        }

        // Download
        public async UniTask<(int currentlyDownloaded, int currentCounter)> DownloadAsset(WezitAssets.Asset asset, int currentlyDownloaded, int currentCounter, string nextAssetTitle, string transformation = "all")
        {
            bool hasTransformation = false;
            if (asset.usages.Contains("maps"))
            {
                transformation = "tiles-zip";
            }
            foreach (WezitAssets.File file in asset.files)
            {
                if ((file.label == transformation) || (transformation == "all"))
                {
                    hasTransformation = true;
                    int downloaded = file.size;
                    if (CheckDownloadNecessity(file))
                    {
                        if (transformation == "tiles-zip")
                        {
                            downloaded = await DownloadMapTiles(file, asset.pid);
                        }
                        else
                        {
                            downloaded = await DownloadFile(file, asset.pid);
                        }

                        currentlyDownloaded += downloaded;
                        DownloadProgress?.Invoke(currentlyDownloaded, nextAssetTitle);
                        currentCounter++;
                    }

                    if (transformation != "all")
                    {
                        continue;
                    }
                }
            }
            if (!hasTransformation)
            {
                WezitAssets.File file = asset.files.Find(x => x.label == "original");

                if (file == null)
                {
                    return (currentlyDownloaded, currentCounter);
                }

                int downloaded = file.size;

                if (CheckDownloadNecessity(file))
                {
                    downloaded = await DownloadFile(file, asset.pid);
                }

                currentlyDownloaded += downloaded;
                DownloadProgress?.Invoke(currentlyDownloaded, nextAssetTitle);
                currentCounter++;
            }
            return (currentlyDownloaded, currentCounter);
        }

        public async UniTask<int> DownloadFile(WezitAssets.File file, string pid)
        {
            return await DownloadFile(file.uri, Path.Combine(AssetsFolderPath, file.path), file.md5, pid);
        }

        public async UniTask<int> DownloadFile(string uri, string path, string md5, string pid, bool saveMd5Dictionnary = false)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(uri);
            await webRequest.SendWebRequest();
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("WezitDownloader - DownloadImage - Error when downloading " + uri + " : " + webRequest.error);
                return 0;
            }

            byte[] imageBytes = webRequest.downloadHandler.data;
            using FileStream fileStream = File.Create(path);
            fileStream.Write(imageBytes);

            AssetAndMd5 assetAndMd5 = DownloadedAssetsMd5Dict.Find(x => x.path == path);
            if (assetAndMd5 != null)
            {
                DownloadedAssetsMd5Dict[DownloadedAssetsMd5Dict.IndexOf(assetAndMd5)].md5 = md5;
            }
            else
            {
                DownloadedAssetsMd5Dict.Add(new AssetAndMd5(pid, path, md5));
            }

            if (saveMd5Dictionnary)
            {
                Save();
            }

            return (int)webRequest.downloadedBytes;
        }

        private async UniTask<int> DownloadMapTiles(WezitAssets.File file, string pid)
        {
            if (!Directory.Exists(Path.Combine(AssetsFolderPath, Path.GetDirectoryName(file.path))))
            {
                Directory.CreateDirectory(Path.Combine(AssetsFolderPath, Path.GetDirectoryName(file.path)));
            }
            await UniRxZipDownloader.DownloadAndUnzip(file.uri, Path.Combine(AssetsFolderPath, file.path));

            DownloadedAssetsMd5Dict.Add(new AssetAndMd5(pid, Path.Combine(AssetsFolderPath, file.path), file.md5));
            return file.size;
        }

        // Delete
        public void DeleteImages()
        {
            foreach (var directory in Directory.GetDirectories(AssetsFolderPath))
            {
                DirectoryInfo data_dir = new DirectoryInfo(directory);
                data_dir.Delete(true);
            }

            foreach (var file in Directory.GetFiles(AssetsFolderPath))
            {
                FileInfo file_info = new FileInfo(file);
                file_info.Delete();
            }
            HasDownloaded = false;
            m_assets.Clear();
        }

        public void AbortDownload()
        {
            HasDownloaded = false;
            m_stopDownload = true;
            DownloadedAssetsMd5Dict.Clear();
            DeleteImages();
        }

        #region Save/Load behavior
        const string CONST_FILE_NAME = "assets_md5.dat";

        private static string FilePath
        {
            get
            {
                return Path.Combine(AssetsFolderPath, CONST_FILE_NAME);
            }
        }

        public void Load(bool a_reset = false)
        {

            if (a_reset || m_loaded)
                return;

            FileStream file;

            if (File.Exists(FilePath))
            {
                file = File.OpenRead(FilePath);
                BinaryFormatter bf = new BinaryFormatter();
                string data = (string)bf.Deserialize(file);
                file.Close();
                JsonUtility.FromJsonOverwrite(data, this);

                m_loaded = true;

                Debug.Log("Assets data loaded");
            }
            else
            {
                m_loaded = false;
                Debug.Log("There is no assets data to load");
            }
        }

        public void Save()
        {
            m_fileContent = JsonUtility.ToJson(this, true);
            m_savingThread = new Thread(SaveData);
            m_filePath = FilePath;
            if (!m_saving) m_savingThread.Start();
        }

        public void SaveData()
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

            Debug.Log("Assets data saved");
            m_saving = false;
            m_savingThread.Abort();
        }
        #endregion
        #endregion
    }
}
