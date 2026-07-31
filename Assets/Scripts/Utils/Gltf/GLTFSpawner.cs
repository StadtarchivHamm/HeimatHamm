using System;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using GLTFast;
using System.Threading.Tasks;

namespace Utils
{
	public static class GLTFSpawner
	{
		public static async Task<GameObject> SpawnGLTF(Transform root, Wezit.Node wzData)
		{
			string source = "";
			if (wzData == null)
			{
				Debug.LogError("Poi is null");
			}

			await wzData.GetRelations();
			if (wzData.ThreeDRelations?.Count == 0)
			{
				Debug.LogError("No 3D object for poi " + wzData.pid);
				return null;
			}

			source = wzData?.ThreeDRelations[0].GetAssetSourceByTransformation(WezitSourceTransformation.default_base);

			if (string.IsNullOrEmpty(source))
			{
				Debug.LogError("Source is empty for POI " + wzData.pid);
				return null;
			}

			GltfImport gltfImport = new GltfImport();
			await gltfImport.Load(source);

			GameObject SceneRoot = new GameObject();
			SceneRoot.transform.parent = root;
			SceneRoot.transform.localPosition = Vector3.zero;
			SceneRoot.transform.localRotation = Quaternion.identity;
			SceneRoot.name = wzData.title;

			GameObjectInstantiator instantiator = new GameObjectInstantiator(gltfImport, SceneRoot.transform);
			bool success = await gltfImport.InstantiateMainSceneAsync(instantiator);
			if (success)
			{
				// Get the SceneInstance to access the instance's properties
				GameObjectSceneInstance sceneInstance = instantiator.SceneInstance;

				// Play the default (i.e. the first) animation clip
				Animation legacyAnimation = sceneInstance.LegacyAnimation;
				if (legacyAnimation != null)
				{
					legacyAnimation.Play();
				}

				return instantiator.SceneTransform.gameObject;
			}
			else
			{
				Debug.LogError("GLB instantiation was a failure");
				return null;
			}
		}
	}
}
