/**
 * Created by Willy
 */

using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UniRx.Async;

namespace Utils
{
	public class FileUtils : MonoBehaviour
	{
		private static string TAG = "FileUtils";
		private static string TAG_REQUEST_CONTENT = "<color=aqua>RequestContent :</color>";
		private static string TAG_SAVE_FILE = "<color=yellow>saveFile :</color>";

		public static IEnumerator RequestDataContent(string _url, int timeout = 0, Action<byte[]> getBytesCallBack = null)
		{
			Debug.Log(TAG + " " + TAG_REQUEST_CONTENT + _url);
			UnityWebRequest www = UnityWebRequest.Get(_url);
			www.timeout = timeout;

			yield return www.SendWebRequest();

			if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError(TAG + " " + TAG_REQUEST_CONTENT + " -ERROR " + www.error + " for url: " + _url);
				getBytesCallBack(null);
			}
			else
			{
				Debug.Log(TAG + " " + TAG_REQUEST_CONTENT + " -OK");
				getBytesCallBack(www.downloadHandler.data);
			}
		}

		public static async UniTask<string> RequestTextContent(string _url, int timeout)
		{
			Debug.Log(TAG + " " + TAG_REQUEST_CONTENT + _url);
#if UNITY_IOS && !UNITY_EDITOR
			if (_url.Contains("://") == false)
			{
				_url = "file://" + _url;
			}
#endif
			UnityWebRequest www = UnityWebRequest.Get(_url);
			www.timeout = timeout;

			await www.SendWebRequest();

			if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.LogError(TAG + " " + TAG_REQUEST_CONTENT + " -ERROR" + www.error + " for url: " + _url);
				return null;
			}
			else
			{
				return www.downloadHandler.text;
			}
		}

		public static void saveFile(string filePath, byte[] bytes)
		{
			Debug.Log(TAG + " " + TAG_SAVE_FILE + filePath + " / file size : " + bytes.Length);
			FileStream file = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite);
			BinaryWriter binaryWriter = new BinaryWriter(file);
			binaryWriter.Write(bytes);
			file.Close();
		}

		public static string calculateMD5Hash(string filePath)
		{
			if (!isFileUnavailable(filePath))
			{
				FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				byte[] byteArray = md5.ComputeHash(file);
				file.Close();

				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < byteArray.Length; i++)
					stringBuilder.Append(byteArray[i].ToString("x2")); // prints the byte in Hexadecimal

				return stringBuilder.ToString();
			}
			else
			{
				return null;
			}
		}

		public static bool isFileUnavailable(string path)
		{
			// if file doesn't exist, return true
			if (!File.Exists(path))
				return true;

			FileInfo file = new FileInfo(path);
			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				// - still being written to
				// - or being processed by another thread
				// - or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			//file is not locked
			return false;
		}

		public static IEnumerator waitFileAvailable(string filePath, float waitValue = .05f)
		{
			while (isFileUnavailable(filePath))
				yield return new WaitForSeconds(waitValue);
		}

		public static string GetImagePathFromStreamingAssets(string folder, string imgName, string fileExtension)
		{
			return Application.streamingAssetsPath + "/" + folder + "/" + imgName + "." + fileExtension;
		}
	}
}
