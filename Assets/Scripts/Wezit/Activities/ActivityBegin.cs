using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using TMPro;
using UnityEngine.Video;

namespace Wezit
{
    public class ActivityBegin : MonoBehaviour
    {
        #region Fields
        #region SerializeFields
        [SerializeField] private RawImage _beginImage = null;
        [SerializeField] private TextMeshProUGUI _beginTitle = null;
        [SerializeField] private TextMeshProUGUI _beginDescription = null;
        [SerializeField] private VideoPlayer _beginVideoPlayer;
        [SerializeField] private AudioSource _beginAudioPlayer;
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

            if (_beginTitle) _beginTitle.text = StringUtils.CleanFromWezit(m_ActivityNode[language.ToString()]["template.activity.begin.title.text.content"]);
            if (_beginDescription) _beginDescription.text = StringUtils.CleanFromWezit(m_ActivityNode[language.ToString()]["template.activity.begin.description.text.content"]);

            if (_beginImage)
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
            string imageName = StringUtils.CleanFromWezit(m_ActivityNode[language.ToString()]["template.activity.begin.image"]);
            if (!string.IsNullOrEmpty(imageName))
            {
                imageName = imageName.Replace("wzasset://", "");
                WezitAssets.Asset asset = AssetsLoader.GetAssetById(imageName);
                StartCoroutine(Utils.ImageUtils.SetImage(_beginImage,
                                                         asset.GetAssetSourceByTransformation(WezitSourceTransformation.default_base),
                                                         asset.GetAssetMimeTypeByTransformation(WezitSourceTransformation.default_base),
                                                         true));
            }
        }
        #endregion
        #endregion
    }
}
