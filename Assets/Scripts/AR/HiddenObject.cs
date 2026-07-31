using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HiddenObject : MonoBehaviour
{
    #region Fields
    #region SerializeFields

    #endregion
    #region Private

    #endregion
    #endregion

    #region Properties
    public UnityEvent<HiddenObject> ObjectClicked = new UnityEvent<HiddenObject>();
    #endregion

    #region Methods
    #region Monobehaviours
    private void OnMouseDown()
    {
        ObjectClicked?.Invoke(this);
    }
    #endregion
    #region Public
    public HiddenObject Init()
    {
        MeshRenderer childRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        float maxSizeMagnitude = childRenderer.bounds.size.magnitude;

        MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material.SetFloat("metallicFactor", 0.3f);

            float meshSize = meshRenderer.bounds.size.magnitude;
            if (meshSize > maxSizeMagnitude)
            {
                maxSizeMagnitude = meshSize;
                childRenderer = meshRenderer;
            }
        }
        childRenderer.gameObject.AddComponent<BoxCollider>();
        childRenderer.gameObject.AddComponent<ColliderClicked>().Clicked.AddListener(OnChildColliderClicked);

        return this;
    }

    public void Disappear()
    {
        gameObject.SetActive(false);
    }
    #endregion
    #region Private
    private void OnChildColliderClicked()
    {
        ObjectClicked?.Invoke(this);
    }
    #endregion
    #endregion
}
