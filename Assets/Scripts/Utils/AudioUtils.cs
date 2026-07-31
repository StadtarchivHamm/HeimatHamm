using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Networking;

public class AudioUtils : MonoBehaviour
{
	public async static UniTask<string> GetAudioSource(Wezit.Node wezitData, int index = 0, string tag = "", bool comesFromGetAmbiant = false)
	{
		if (wezitData == null)
		{
			return null;
		}

		await wezitData.AreRelationsSet();
		if (wezitData.AudioRelations == null || wezitData.AudioRelations.Count == 0)
        {
            Debug.LogWarning("No audio relation for POI " + wezitData.pid + ", will try and return ambiant audio relation.");
			return comesFromGetAmbiant ? null : await GetAmbiantAudioSource(wezitData, index, tag, true);
		}

		if (!string.IsNullOrEmpty(tag))
		{
			return wezitData.AudioRelations.Find(audio => audio.tags.Contains(tag)).GetAssetSourceByTransformation(WezitSourceTransformation.default_base);
		}

		return wezitData.AudioRelations[index < wezitData.AudioRelations.Count ? index : 0].GetAssetSourceByTransformation(WezitSourceTransformation.default_base);
	}

	public async static UniTask<string> GetAmbiantAudioSource(Wezit.Node wezitData, int index = 0, string tag = "", bool comesFromGetAudio = false)
	{
		if (wezitData == null)
		{
			Debug.LogWarning("Node is null");
			return null;
		}

		await wezitData.AreRelationsSet();
		if (wezitData.AmbiantAudioRelations == null || wezitData?.AmbiantAudioRelations.Count == 0)
		{
			Debug.LogWarning("No ambiant audio relation for POI " + wezitData.pid + ", will try and return audio relation.");
			return comesFromGetAudio ? null : await GetAudioSource(wezitData, index, tag, true);
		}

		if (!string.IsNullOrEmpty(tag))
		{
			return wezitData.AmbiantAudioRelations.Find(audio => audio.tags.Contains(tag)).GetAssetSourceByTransformation(WezitSourceTransformation.default_base);
		}

		return wezitData.AmbiantAudioRelations[index < wezitData.AmbiantAudioRelations.Count ? index : 0].GetAssetSourceByTransformation(WezitSourceTransformation.default_base);
	}

	public static async UniTask<AudioClip> GetAudioClip(Wezit.Node wezitData, int index = 0, string tag = "")
	{
		return await GetAudioClip(await GetAudioSource(wezitData, index, tag));
	}

	public static async UniTask<AudioClip> GetAudioClip(string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			Debug.LogWarning("Source is null or empty");
			return null;
		}

		UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(source, AudioType.MPEG);
		await www.SendWebRequest();

		if (www.result == UnityWebRequest.Result.ConnectionError)
		{
			Debug.Log(www.error);
			return null;
		}
		else return (DownloadHandlerAudioClip.GetContent(www));
	}

	public static async UniTask<bool> NodeHasAudioClip(Wezit.Node wezitData)
	{
		return !string.IsNullOrEmpty(await GetAudioSource(wezitData));
	}
}
