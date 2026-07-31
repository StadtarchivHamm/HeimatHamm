using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using TMPro;
using UnityEngine.Video;

namespace Wezit
{
    public class ActivityEnd : MonoBehaviour
    {
        #region Fields
        #region SerializeFields
        [SerializeField] private RawImage _endImage = null;
        [SerializeField] private TextMeshProUGUI _endTitle = null;
        [SerializeField] private TextMeshProUGUI _endDescription = null;
        [SerializeField] private VideoPlayer _endVideoPlayer = null;
        [SerializeField] private AudioSource _endAudioPlayer = null;
        #endregion
        #region Private
        private JSONNode m_ActivityNode;
        private string m_VideoUrl;
        private string m_AudioUrl;
        private string m_TitleTextStyle;
        private string m_DescriptionTextStyle;
        #endregion
        #endregion

        #region Properties
        #endregion

        #region Methods
        #region Monobehaviour
        #endregion
        #region Public
        public void Inflate(JSONNode activityNode, Language language)
        {
            m_ActivityNode = activityNode;

            if (_endTitle) _endTitle.text = StringUtils.CleanFromWezit(m_ActivityNode[language.ToString()]["template.activity.end.title.text.content"]);
            if (_endDescription) _endDescription.text = StringUtils.CleanFromWezit(m_ActivityNode[language.ToString()]["template.activity.end.description.text.content"]);

            if (_endImage)
            {
                LoadImage(language);
            }
        }
        #endregion
        #region Internal
        #endregion
        #region Private
        private void LoadImage(Language language)
        {
            string imageName = StringUtils.CleanFromWezit(m_ActivityNode[language.ToString()]["template.activity.end.image"]);
            if (!string.IsNullOrEmpty(imageName))
            {
                imageName = imageName.Replace("wzasset://", "");
                WezitAssets.Asset asset = AssetsLoader.GetAssetById(imageName);
                StartCoroutine(Utils.ImageUtils.SetImage(_endImage,
                                                         asset.GetAssetSourceByTransformation(WezitSourceTransformation.original),
                                                         asset.GetAssetMimeTypeByTransformation(WezitSourceTransformation.original),
                                                         true));
            }
        }
        #endregion
        #endregion
    }
}
