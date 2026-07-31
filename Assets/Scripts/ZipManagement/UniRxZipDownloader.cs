using UnityEngine;
using UniRx;
using UnityEngine.Networking;
using UniRx.Async;

public static class UniRxZipDownloader
{
	public static async UniTask<bool> DownloadAndUnzip(string source, string path) 
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(source))
        {
            await webRequest.SendWebRequest();

			if (string.IsNullOrEmpty(webRequest.error))
			{
				if (webRequest.downloadHandler.data != null)
				{
					byte[] bytes = webRequest.downloadHandler.data;
					Debug.Log("ZipDownloadManager - Download successfully completed");

					ZipFile.UnZip(path, bytes);
					return true;
				}
				else
				{
					Debug.LogWarning("ZipDownloadManager - File does not exist: " + source + "\ndata is null: " + (webRequest.downloadHandler.data == null).ToString());
					return false;
				}

			}
			else
			{
				Debug.LogError("ZipDownloadManager - Error when downloading " + source + ": " + webRequest.error);
				return false;
			}
        }
	}
	
}
