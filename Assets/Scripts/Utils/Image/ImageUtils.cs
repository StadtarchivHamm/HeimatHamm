using System;
using UnityEngine.UI;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UniRx.Async;
using Unity.Samples.ScreenReader;
using System.Threading.Tasks;
using Wezit;

namespace Utils
{
	public static class ImageUtils
	{
		private const int _imagesLoadBeforeResourceClean = 50;
		private static int _currentImagesLoadCount = 0;

		private static Dictionary<string, IEnumerator> routineDict = new Dictionary<string, IEnumerator>();

		#region Image
		public static IEnumerator SetImage(Image imageComponent, string assetSource, string mimeType = "", bool fillParent = true, float crossFadeAlphaDuration = 0.5f, bool enableImageOnLoaded = true, float alphaOnLoaded = 1)
		{
			if (_currentImagesLoadCount >= _imagesLoadBeforeResourceClean) Resources.UnloadUnusedAssets();
			_currentImagesLoadCount++;

			imageComponent.CrossFadeAlpha(0, 0f, false);

			if (string.IsNullOrEmpty(assetSource))
			{
				ResetImage(imageComponent);
				yield break;
			}

			if (!routineDict.ContainsKey(assetSource + imageComponent.GetInstanceID()))
            {
				IEnumerator getImageSpriteRoutine = TextureAndSpriteUtils.GetSpriteFromSource(assetSource, (result) => ApplySprite(imageComponent, fillParent, result, crossFadeAlphaDuration, enableImageOnLoaded, alphaOnLoaded));
				routineDict.Add(assetSource + imageComponent.GetInstanceID(), getImageSpriteRoutine);
				
				yield return getImageSpriteRoutine;
				
				routineDict.Remove(assetSource + imageComponent.GetInstanceID());
            }
		}

		public static void ResetImage(Image imageComponent)
		{
			if (imageComponent)
			{
				imageComponent.sprite = null;
				imageComponent.enabled = false;
			}
		}

		private static void ApplySprite(Image imageComponent, bool fillParent, Sprite sprite, float crossFadeAlphaDuration = 0.5f, bool enableImageOnLoaded = true, float alphaOnLoaded = 1)
		{
			if (sprite != null && imageComponent != null)
			{
				imageComponent.sprite = sprite;
				imageComponent.enabled = true;
				imageComponent.preserveAspect = true;

				if (imageComponent.GetComponent<AspectRatioFitter>() != null)
				{
					if (fillParent)
					{
						// Fill parent method (centered, don't forget to add a mask parent to crop it)
						imageComponent.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
					}
					else
					{
						// Fit in parent
						imageComponent.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
					}

					imageComponent.GetComponent<AspectRatioFitter>().aspectRatio = (float)Math.Round(sprite.bounds.size.x / sprite.bounds.size.y, 2);
				}
				else
				{
					Debug.LogWarning("[ImageUtils.SetImage] - Need 'AspectRatioFitter' component");
				}

				if (enableImageOnLoaded) imageComponent.CrossFadeAlpha(alphaOnLoaded, crossFadeAlphaDuration, false);
				else imageComponent.enabled = false;
			}
		}
		#endregion

		#region RawImage
		public static IEnumerator SetImage(RawImage imageComponent, string assetSource, string mimeType = "", bool fillParent = true, float crossFadeAlphaDuration = 0.5f, bool enableImageOnLoaded = true, float alphaOnLoaded = 1)
		{

			if (_currentImagesLoadCount >= _imagesLoadBeforeResourceClean) Resources.UnloadUnusedAssets();
			_currentImagesLoadCount++;

			imageComponent.CrossFadeAlpha(0, 0f, false);

			if (string.IsNullOrEmpty(assetSource))
			{
				ResetImage(imageComponent);
				yield break;
			}

			if (!routineDict.ContainsKey(assetSource + imageComponent.GetInstanceID()))
			{
				IEnumerator getImageTextureRoutine = TextureAndSpriteUtils.GetTextureFromSource(assetSource, (result) => ApplyTexture(imageComponent, fillParent, result, crossFadeAlphaDuration, enableImageOnLoaded, alphaOnLoaded));
				routineDict.Add(assetSource + imageComponent.GetInstanceID(), getImageTextureRoutine);

				yield return getImageTextureRoutine;

				routineDict.Remove(assetSource + imageComponent.GetInstanceID());
			}
		}

		public static void ResetImage(RawImage imageComponent)
		{
			if (imageComponent)
			{
				imageComponent.texture = null;
				imageComponent.enabled = false;
			}
		}

		private static void ApplyTexture(RawImage imageComponent, bool fillParent, Texture texture, float crossFadeAlphaDuration = 0.5f, bool enableImageOnLoaded = true, float alphaOnLoaded = 1)
		{
			if (texture != null && imageComponent != null)
			{
				imageComponent.texture = texture;
				imageComponent.enabled = true;

				if (imageComponent.GetComponent<ImageAspectRatioSetter>() == null)
				{
					imageComponent.gameObject.AddComponent<ImageAspectRatioSetter>().InitSetter();
				}

				if (imageComponent.TryGetComponent(out AspectRatioFitter aspectRatioFitter))
				{
					if (fillParent)
					{
						// Fill parent method (centered and cropped)
						aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
					}
					else
					{
						// Fit in parent
						aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
					}
				}

				if (enableImageOnLoaded) imageComponent.CrossFadeAlpha(alphaOnLoaded, crossFadeAlphaDuration, false);
				else imageComponent.enabled = false;
			}
		}

		public async static Task<Relation> LoadImage(RawImage imageComponent, MonoBehaviour monoBehaviourInstance, Wezit.Node wzData, int index = 0, string targetWzSourceTransformation = "default", bool fillParent = true, float crossFadeAlphaDuration = 0.25f, bool enableOnLoaded = true, float alphaOnLoaded = 1f)
		{
			if (imageComponent != null)
			{
				if (wzData != null)
				{
					await wzData.AreRelationsSet();

					if (wzData.ShowPictureRelations == null || wzData.ShowPictureRelations.Count == 0)
					{
						ResetImage(imageComponent);
						return null;
					}

					Relation relation = wzData.ShowPictureRelations[wzData.ShowPictureRelations.Count > index ? index : 0];
					if (!string.IsNullOrEmpty(relation.description))
					{
						if (imageComponent.TryGetComponent(out AccessibleImage accessibleImage))
						{
							accessibleImage.SetLabel(relation.CleanedDescription);
							accessibleImage.value = relation.CleanedDescription;
							monoBehaviourInstance.DelayRefreshHierarchy();
						}
					}

					if (!monoBehaviourInstance.gameObject.activeInHierarchy)
					{
						return null;
					}

					monoBehaviourInstance.StartCoroutine(SetImage(
						imageComponent,
						relation.GetAssetSourceByTransformation(targetWzSourceTransformation),
						relation.GetAssetMimeTypeByTransformation(targetWzSourceTransformation),
						fillParent,
						crossFadeAlphaDuration,
						enableOnLoaded,
						alphaOnLoaded));

					return relation;
				}
				else
				{
					ResetImage(imageComponent);
					return null;
				}
			}
			return null;
		}

		public async static void LoadRefImage(RawImage imageComponent, MonoBehaviour monoBehaviourInstance, Node wzData, int index = 0, string targetWzSourceTransformation = "default", bool fillParent = true, float crossFadeAlphaDuration = 0.25f, bool enableOnLoaded = true, float alphaOnLoaded = 1f)
		{
			if (imageComponent == null)
			{
				Debug.LogWarning("The image component is null");
				return;
			}
			if (wzData == null)
			{
				Debug.LogWarning("Wezit node is null");
				ResetImage(imageComponent);
				return;
			}

            await wzData.AreRelationsSet();

			if (wzData.RefPictureRelations == null || wzData.RefPictureRelations?.Count == 0)
			{
				Debug.LogWarning("No refPicture relation for POI " + wzData.pid + ", loading showPicture relation");
				LoadImage(imageComponent, monoBehaviourInstance, wzData, index, targetWzSourceTransformation, fillParent, crossFadeAlphaDuration, enableOnLoaded, alphaOnLoaded);
				return;
			}

			Relation relation = wzData.RefPictureRelations[Mathf.Min(wzData.RefPictureRelations.Count - 1, index)];
			monoBehaviourInstance.StartCoroutine(SetImage(
				imageComponent,
				relation.GetAssetSourceByTransformation(targetWzSourceTransformation),
				relation.GetAssetMimeTypeByTransformation(targetWzSourceTransformation),
				fillParent,
				crossFadeAlphaDuration,
				enableOnLoaded,
				alphaOnLoaded));
		}

		public async static void LoadCover(RawImage imageComponent, MonoBehaviour monoBehaviourInstance, Wezit.Node wzData, string targetRelation, int index = 0, string targetWzSourceTransformation = "default", bool envelopeParent = true, float crossFadeAlphaDuration = 0.25f)
		{
			if (imageComponent != null)
			{
				if (wzData != null)
				{
					bool hasMedia = false;
					bool hasCover = false;

					await wzData.AreRelationsSet();
                    if (await wzData.HasRelationOfType(targetRelation))
                    {

                    }
					foreach (Wezit.Relation relation in wzData.Relations)
					{
						if (relation.relation == targetRelation)
						{
							hasMedia = true;
							Wezit.WezitAssets.Asset asset = CoverStore.GetCoverAssetForPid(relation.pid);
							if (asset != null)
							{
								hasCover = true;
								monoBehaviourInstance.StartCoroutine(SetImage(
									imageComponent,
									asset.GetAssetSourceByTransformation(targetWzSourceTransformation),
									asset.GetAssetMimeTypeByTransformation(targetWzSourceTransformation),
									envelopeParent,
									crossFadeAlphaDuration,
									true,
									1));
								break;
							}
						}
					}
					if (!hasMedia || !hasCover) ResetImage(imageComponent);
				}
				else
				{
					ResetImage(imageComponent);
				}
			}
		}

		public static async UniTask<bool> HasCover(Wezit.Node wzData, string targetRelation)
		{
			bool hasCover = false;
			if (wzData != null)
			{
				await wzData.GetRelations();
				foreach (Wezit.Relation relation in wzData.Relations)
				{
					if (relation.relation == targetRelation)
					{
						Wezit.WezitAssets.Asset asset = CoverStore.GetCoverAssetForPid(relation.pid);
						if (asset != null)
						{
							hasCover = true;
							break;
						}
					}
				}
			}
			return hasCover;
		}

		public static async UniTask<bool> DownloadImageWithTransformation(Wezit.Node wzData, string targetRelation, string targetTransformation)
		{
			Wezit.WezitAssets.File file = null;
			bool success = false;

			if (wzData != null)
			{
				await wzData.GetRelations();
				foreach (Wezit.Relation relation in wzData.Relations)
				{
					if (relation.relation == targetRelation)
					{
						file = relation.GetAssetByTransformation(targetTransformation);
						if (file != null)
						{
							if (Wezit.DataGrabber.Instance.CheckDownloadNecessity(file))
							{
								await Wezit.DataGrabber.Instance.DownloadFile(file, relation.pid);
								Wezit.DataGrabber.Instance.Save();
								break;
							}
						}
					}
				}

				if (file == null)
				{
					if (targetRelation != Wezit.RelationName.SHOW_PICTURE)
					{
						foreach (Wezit.Relation relation in wzData.Relations)
						{
							if (relation.relation == Wezit.RelationName.SHOW_PICTURE)
							{
								file = relation.GetAssetByTransformation(targetTransformation);

								if (file != null)
								{
									if (Wezit.DataGrabber.Instance.CheckDownloadNecessity(file))
									{
										await Wezit.DataGrabber.Instance.DownloadFile(file, relation.pid);
										Wezit.DataGrabber.Instance.Save();
										break;
									}
								}
							}
						}
					}
				}
			}

			return success;
		}
		#endregion
	}
}
