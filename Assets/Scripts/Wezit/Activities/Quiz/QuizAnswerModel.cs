using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Wezit
{
    public class QuizAnswerModel
    {
        public string AnswerText;
        public string ImageSource;
        public bool IsCorrect;

        public bool ShowIntermediateScreen;
        public string IntermediateScreenTitle;
        public string IntermediateScreenDescription;
        public string IntermediateScreenImageSource;

        public QuizAnswerModel(string answerText, string imageSource, bool isCorrect, bool showIntermediateScreen, string intermediateScreenTitle, string intermediateScreenDescription, string intermediateScreenImageSource)
        {
            ImageSource = imageSource;
            AnswerText = answerText;
            IsCorrect = isCorrect;

            ShowIntermediateScreen = showIntermediateScreen;
            IntermediateScreenTitle = intermediateScreenTitle;
            IntermediateScreenDescription = intermediateScreenDescription;
            IntermediateScreenImageSource = intermediateScreenImageSource;
        }

        public async Task LoadQuestionImage(RawImage questionRawImage, MonoBehaviour monoBehaviour)
        {
            string imageName = ImageSource.Replace("wzasset://", "");
            WezitAssets.Asset asset = AssetsLoader.GetAssetById(imageName);
            await monoBehaviour.StartCoroutine(Utils.ImageUtils.SetImage(questionRawImage,
                                                     asset.GetAssetSourceByTransformation(WezitSourceTransformation.default_base),
                                                     asset.GetAssetMimeTypeByTransformation(WezitSourceTransformation.default_base),
                                                     true));
        }

        public async Task LoadIntermediateImage(RawImage intermediateRawImage, MonoBehaviour monoBehaviour)
        {
            string imageName = IntermediateScreenImageSource.Replace("wzasset://", "");
            WezitAssets.Asset asset = AssetsLoader.GetAssetById(imageName);
            await monoBehaviour.StartCoroutine(Utils.ImageUtils.SetImage(intermediateRawImage,
                                                     asset.GetAssetSourceByTransformation(WezitSourceTransformation.default_base),
                                                     asset.GetAssetMimeTypeByTransformation(WezitSourceTransformation.default_base),
                                                     true));
        }
    }
}
