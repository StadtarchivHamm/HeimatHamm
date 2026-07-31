using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ThreeDManager : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private InteractiveObjectRotator _objectRotator;
	[SerializeField] private Transform _itemRoot;
	#endregion
	#region Private
	#endregion
	#endregion

	#region Properties
	#endregion

	#region Methods
	#region Monobehaviours
	#endregion

	#region Public
	public async void Inflate(Wezit.Poi poi)
	{
        _itemRoot.localScale = Vector3.one;

        foreach (Transform child in _itemRoot)
        {
			if(child.TryGetComponent(out Light light))
            {
				continue;
            }

			Destroy(child.gameObject);
        }

		GameObject model = await Utils.GLTFSpawner.SpawnGLTF(_itemRoot, poi);

		MeshRenderer[] meshRenderers = model.GetComponentsInChildren<MeshRenderer>();
		Vector3 sizeVector = Vector3.negativeInfinity;

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
			meshRenderer.material.SetFloat("metallicFactor", 0.3f);

			sizeVector.x = Mathf.Max(sizeVector.x, meshRenderer.localBounds.size.x);
			sizeVector.y = Mathf.Max(sizeVector.y, meshRenderer.localBounds.size.y);
			sizeVector.z = Mathf.Max(sizeVector.z, meshRenderer.localBounds.size.z);
        }

        float size = Mathf.Max(sizeVector.x, sizeVector.y, sizeVector.z);
		model.transform.localScale = (6.013f / size * 1.7f) * Vector3.one;

		_objectRotator.SetZoomLimits(6.013f / size * 1.7f, 6.013f / size * 0.7f);
		_objectRotator.Init();
	}
	#endregion

	#region Private
	#endregion
	#endregion
}
