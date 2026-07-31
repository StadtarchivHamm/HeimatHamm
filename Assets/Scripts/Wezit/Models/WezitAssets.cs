using System;
using System.Collections.Generic;
using System.IO;

namespace Wezit
{
    [Serializable]
    public class WezitAssets
    {
        [Serializable]
        public class File
        {
            public string path { get; set; }
            public Metadata metadata { get; set; }
            public int size { get; set; }
            public string mimetype { get; set; }
            public string label { get; set; }
            public string uri { get; set; }
            public string url { get; set; }
            public string rootFolder { get; set; }
            public string md5 { get; set; }
        }

        [Serializable]
        public class Metadata
        {
            public string height;
            public string width;
            public string duration;
        }

        [Serializable]
        public class Asset
        {
            public string use { get; set; }
            public Synchronise synchronise { get; set; }
            public List<File> files { get; set; }
            public string pid { get; set; }
            public string language { get; set; }
            public string usages { get; set; }
            public string title { get; set; }

            public string GetAssetSourceByTransformation(string transformation)
            {
                File file = files.Find(x => x.label == transformation);
                if (file == null)
                {
                    file = files.Find(x => x.label == "original");
                }
                string fullPath = Path.Combine(DataGrabber.AssetsFolderPath, file.path);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		    	fullPath = "file://" + fullPath;
#endif
                return !DataGrabber.Instance.CheckDownloadNecessity(fullPath.Replace("file://", ""), file.md5) ? fullPath : file.url;
            }

            public string GetAssetMimeTypeByTransformation(string transformation)
            {
                File file = files.Find(x => x.label == transformation);
                if (file == null)
                {
                    file = files.Find(x => x.label == "original");
                }
                return file.mimetype;
            }
        }

        [Serializable]
        public class Synchronise
        {
            public string defaultmode { get; set; }
            public string usermode { get; set; }
        }
    }
}
