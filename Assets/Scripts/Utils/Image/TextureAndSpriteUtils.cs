using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using UniRx.Async;
using System.IO;

public class TextureAndSpriteUtils
{
	private static Dictionary<string, UnityWebRequest> m_webRequestDict = new Dictionary<string, UnityWebRequest>();
	private static Dictionary<string, UnityWebRequestAsyncOperation> m_webRequestAsyncOpDict = new Dictionary<string, UnityWebRequestAsyncOperation>();
	// webrequests have to be disposed once they're used, but we have to track if they are still being used before doing so
	private static List<string> m_reusedWebRequestCounter = new List<string>();

	public static IEnumerator GetSpriteFromSource(string source, Action<Sprite> result, float pivotX = 0, float pivotY = 0)
	{
		Texture2D texture = new Texture2D(1, 1);

		Debug.Log("TextureAndSpriteUtils - GetSpriteFromSource - uri : " + source);
		if (!m_webRequestDict.ContainsKey(source))
		{
			Debug.Log("TextureAndSpriteUtils - GetSpriteFromSource - uri : " + source);
			m_webRequestDict.Add(source, UnityWebRequest.Get(source));
		}

		UnityWebRequest webRequest = m_webRequestDict[source];

		if (!m_webRequestAsyncOpDict.ContainsKey(source))
		{
			m_webRequestAsyncOpDict.Add(source, webRequest.SendWebRequest());
		}
		else
		{
			m_reusedWebRequestCounter.Add(source);
		}

		yield return m_webRequestAsyncOpDict[source];

		m_webRequestDict.Remove(source);
		m_webRequestAsyncOpDict.Remove(source);

		if (string.IsNullOrEmpty(webRequest.error))
		{
			if (texture != null && webRequest.downloadHandler.data != null)
			{
				texture.LoadImage(webRequest.downloadHandler.data);
				result(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(pivotX, pivotY), 100, 0, SpriteMeshType.FullRect));
			}
			else
			{
				Debug.Log("TextureAndSpriteUtils - GetSpriteFromSource - File does not exist: " + source);
				result(Resources.Load<Sprite>("Images/DefaultImage/default"));
			}

		}
		else
		{
			Debug.LogError("TextureAndSpriteUtils - GetSpriteFromSource - Error when downloading " + source + ": " + webRequest.error);
			result(Resources.Load<Sprite>("Images/DefaultImage/default"));
			yield break;
		}

		// Check if another coroutine is using this webRequest before disposing it
		if (m_reusedWebRequestCounter.Find(reusedSource => reusedSource == source) != null)
		{
			m_reusedWebRequestCounter.Remove(source);
		}
		else
		{
			webRequest.Dispose();
		}
		Resources.UnloadUnusedAssets();
    }

    public static IEnumerator GetSpriteFromSource(string source, Action<Sprite, int> result, int index, float pivotX = 0, float pivotY = 0)
    {
        Texture2D texture = new Texture2D(1, 1);

        Debug.Log("TextureAndSpriteUtils - GetSpriteFromSource - uri : " + source);
        if (!m_webRequestDict.ContainsKey(source))
        {
            Debug.Log("TextureAndSpriteUtils - GetSpriteFromSource - uri : " + source);
            m_webRequestDict.Add(source, UnityWebRequest.Get(source));
        }

        UnityWebRequest webRequest = m_webRequestDict[source];

        if (!m_webRequestAsyncOpDict.ContainsKey(source))
        {
            m_webRequestAsyncOpDict.Add(source, webRequest.SendWebRequest());
        }
        else
        {
            m_reusedWebRequestCounter.Add(source);
        }

        yield return m_webRequestAsyncOpDict[source];

        m_webRequestDict.Remove(source);
        m_webRequestAsyncOpDict.Remove(source);

        if (string.IsNullOrEmpty(webRequest.error))
        {
            if (texture != null && webRequest.downloadHandler.data != null)
            {
                texture.LoadImage(webRequest.downloadHandler.data);
                result(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(pivotX, pivotY), 100, 0, SpriteMeshType.FullRect), index);
            }
            else
            {
                Debug.Log("TextureAndSpriteUtils - GetSpriteFromSource - File does not exist: " + source);
                result(Resources.Load<Sprite>("Images/DefaultImage/default"), index);
            }

        }
        else
        {
            Debug.LogError("TextureAndSpriteUtils - GetSpriteFromSource - Error when downloading " + source + ": " + webRequest.error);
            result(Resources.Load<Sprite>("Images/DefaultImage/default"), index);
            yield break;
        }

        // Check if another coroutine is using this webRequest before disposing it
        if (m_reusedWebRequestCounter.Find(reusedSource => reusedSource == source) != null)
        {
            m_reusedWebRequestCounter.Remove(source);
        }
        else
        {
            webRequest.Dispose();
        }
        Resources.UnloadUnusedAssets();
    }

    public static IEnumerator GetTextureFromSource(string source, Action<Texture2D> result)
	{
		Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

		if (!m_webRequestDict.ContainsKey(source))
		{
			m_webRequestDict.Add(source, UnityWebRequest.Get(source));
			//Debug.Log("TextureAndSpriteUtils - GetTextureFromSource - uri : " + source);
		}
		else
		{
			m_reusedWebRequestCounter.Add(source);
		}

		UnityWebRequest webRequest = m_webRequestDict[source];

		if (!m_webRequestAsyncOpDict.ContainsKey(source))
		{
			m_webRequestAsyncOpDict.Add(source, webRequest.SendWebRequest());
			yield return m_webRequestAsyncOpDict[source];
		}
		else
		{
			// The WebRequestAsyncOp cannot be yielded by several coroutines, so we have to resort to a simple while true
			while (!webRequest.isDone)
			{
				yield return null;
			}
		}

		m_webRequestDict.Remove(source);
		m_webRequestAsyncOpDict.Remove(source);

		if (!string.IsNullOrEmpty(webRequest.error))
		{
			Debug.LogError("TextureAndSpriteUtils - GetSpriteFromSource - Error when downloading " + source + ": " + webRequest.error);
			result(Resources.Load<Texture2D>("Images/DefaultImage/default"));

			yield break;
		}
		else
		{
			if (texture != null && webRequest.downloadHandler.data != null)
			{
				texture.LoadImage(webRequest.downloadHandler.data);
				texture.Apply();
				result(texture);
			}
			else
			{
				Debug.Log("TextureAndSpriteUtils - GetTextureFromSource - File does not exist: " + source);
				result(Resources.Load<Texture2D>("Images/DefaultImage/default"));
			}
		}

		// Check if another coroutine is using this webRequest before disposing it
		if (m_reusedWebRequestCounter.Find(reusedSource => reusedSource == source) != null)
		{
			m_reusedWebRequestCounter.Remove(source);
		}
		else
		{
			webRequest.Dispose();
		}

		Resources.UnloadUnusedAssets();
	}

	public static async UniTask<Texture2D> GetTextureFromSource(string source)
	{
		Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

		if (!m_webRequestDict.ContainsKey(source))
		{
			m_webRequestDict.Add(source, UnityWebRequest.Get(source));
			Debug.Log("TextureAndSpriteUtils - GetTextureFromSourceAsync - uri : " + source);
		}
		else
		{
			m_reusedWebRequestCounter.Add(source);
		}

		UnityWebRequest webRequest = m_webRequestDict[source];

		if (!m_webRequestAsyncOpDict.ContainsKey(source))
		{
			m_webRequestAsyncOpDict.Add(source, webRequest.SendWebRequest());
			await m_webRequestAsyncOpDict[source];
		}
		else
		{
			// The WebRequestAsyncOp cannot be yielded by several coroutines, so we have to resort to a simple while true
			while (!webRequest.isDone)
			{
				await UniTask.Yield();
			}
		}

		m_webRequestDict.Remove(source);
		m_webRequestAsyncOpDict.Remove(source);

		if (!string.IsNullOrEmpty(webRequest.error))
		{
			Debug.LogError("TextureAndSpriteUtils - GetSpriteFromSource - Error when downloading " + source + ": " + webRequest.error);

			return null;
		}
		else
		{
			if (texture != null && webRequest.downloadHandler.data != null)
			{
				texture.LoadImage(webRequest.downloadHandler.data);
				texture.Apply();
				return texture;
			}
			else
			{
				Debug.Log("TextureAndSpriteUtils - GetTextureFromSource - File does not exist: " + source);
				return Resources.Load<Texture2D>("Images/DefaultImage/default");
			}
		}
	}


	public static async UniTask SaveTextureFromSource(string source, string path, string fileName)
	{
		Texture2D texture = await GetTextureFromSource(source);
		byte[] bytes = texture.EncodeToPNG();
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		File.WriteAllBytes(Path.Combine(path, fileName + ".png"), bytes);
	}

}
