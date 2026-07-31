using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARImageSupport : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private MeshRenderer _rectoMeshRenderer;
    [SerializeField] private Material _rectoMaterial;
    [SerializeField] private MeshRenderer _versoMeshRenderer;
    [SerializeField] private Material _versoMaterial;
    [SerializeField] private float _referenceScaleX;
    [Space]
    [SerializeField] private List<SpriteRenderer> _backImageSpriteRenderers = new List<SpriteRenderer>();
    [SerializeField] private int _backImageReferenceHeight;
    [SerializeField] private int _backImageReferenceWidth;
    [SerializeField] private float _backImageReferenceScale;
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
        await poi.AreRelationsSet();

        StartCoroutine(TextureAndSpriteUtils.GetTextureFromSource(poi.ShowPictureRelations[0].GetAssetSourceByTransformation("default"), SetTexture));

        foreach (SpriteRenderer backSpriteRenderer in _backImageSpriteRenderers)
        {
            backSpriteRenderer.transform.parent.gameObject.SetActive(false);
        }

        if (poi.RefPictureRelations != null && poi.RefPictureRelations.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(poi.RefPictureRelations.Count, _backImageSpriteRenderers.Count); i++)
            {
                _backImageSpriteRenderers[i].transform.parent.gameObject.SetActive(true);
                StartCoroutine(TextureAndSpriteUtils.GetSpriteFromSource(poi.RefPictureRelations[i].GetAssetSourceByTransformation("default"), SetBackSprite, i, 0.5f, 0));
            }
        }
    }
    #endregion
    #region Private
    private void SetTexture(Texture2D texture)
    {
        _rectoMeshRenderer.material = new Material(_rectoMaterial);
        _rectoMeshRenderer.material.mainTexture = texture;

        _rectoMeshRenderer.transform.localScale = new Vector3(_referenceScaleX * (float)texture.width / (float)texture.height * 9 / 16,
                                                              _rectoMeshRenderer.transform.localScale.y, _rectoMeshRenderer.transform.localScale.z);

        _versoMeshRenderer.material = new Material(_versoMaterial);
        _versoMeshRenderer.material.mainTexture = texture;
        _versoMeshRenderer.transform.localScale = _rectoMeshRenderer.transform.localScale;
    }

    private void SetBackSprite(Sprite sprite, int index)
    {
        _backImageSpriteRenderers[index].sprite = sprite;

        float newScale = _backImageReferenceScale * (sprite.texture.width <=  sprite.texture.height ? ((float)_backImageReferenceHeight / sprite.texture.height)
                                                                                                  : ((float)_backImageReferenceWidth / sprite.texture.width));
        _backImageSpriteRenderers[index].transform.localScale = newScale * Vector3.one;
    }
    #endregion
    #endregion
}
