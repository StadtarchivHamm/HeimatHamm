using UnityEngine;
using SimpleJSON;

namespace Unity
{
	public class FileDebugConfig : Singleton<FileDebugConfig>
	{
        #region Fields
        private static string TAG = "<color=red>[UnityFileDebugConfig]</color>";

		private FileDebugConfigModel fileDebugConfig;
		private FileDebug fileDebugObject;
        #endregion

        #region Methods
        public void Init(JSONNode result)
		{
			Debug.Log(TAG + " - Init");

			fileDebugConfig = JsonUtility.FromJson<FileDebugConfigModel>(result["unityFileDebugConfig"].ToString());

			fileDebugObject = gameObject.AddComponent<FileDebug>();
			fileDebugObject.gameObject.SetActive(false);
			fileDebugObject.Init(fileDebugConfig);
			fileDebugObject.gameObject.SetActive(true);
		}
        #endregion
    }
}
