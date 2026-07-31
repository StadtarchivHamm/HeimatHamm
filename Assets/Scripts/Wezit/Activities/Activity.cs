using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.Events;
using UniRx;
using System.Threading.Tasks;

namespace Wezit
{
    public class Activity : MonoBehaviour
    {
        #region Fields
        #region SerializeFields
        [SerializeField] private Transform _beginRoot;
        [SerializeField] private Transform _endRoot;
        #endregion
        #region Private
        private bool m_hasBegin;
        private bool m_hasEnd;
        private float m_chronoValue;

        private ActivityBegin m_begin;
        private ActivityEnd m_end;

        #endregion
        #region Internal
        internal string m_type = ActivityType.DEFAULT;
        internal JSONNode m_activityNode;
        internal JSONNode m_activityNodeInStateLanguage;
        internal Language m_language;
        internal bool m_hasChrono;
        internal string m_BeginPrefabName = "Prefabs/ActivityPrefabs/Begin";
        internal string m_EndPrefabName = "Prefabs/ActivityPrefabs/End";
        #endregion
        #endregion

        #region Properties
        public string Type { get { return m_type; } }
        public UnityEvent ActivityOver = new();
        #endregion

        #region Methods
        #region Monobehaviour
        #endregion
        #region Public
        public virtual void Inflate(JSONNode activityNode, Language language = Language.none)
        {
            if (language == Language.none)
            {
                language = StoreAccessor.State.Language;
            }

            m_activityNode = activityNode;
            m_language = language;
            m_activityNodeInStateLanguage = activityNode[m_language.ToString()];
            m_hasBegin = GetKeyNodeForLanguage(language, "template.activity.begin.activation");
            m_hasEnd = GetKeyNodeForLanguage(language, "template.activity.end.activation");
            m_type = GetKeyNodeForLanguage(language, "template.app.common.type");
            m_hasChrono = GetKeyNodeForLanguage(language, "template.activity.chrono.activation");
            if (m_hasChrono) m_chronoValue = GetKeyNodeForLanguage(language, "template.activity.chrono.value");

            InitContent();
        }

        public virtual void StartActivity()
        {

        }
        #endregion
        #region Internal
        #endregion
        #region Private
        internal virtual void InitContent()
        {
            if (m_hasBegin && _beginRoot)
            {
                m_begin = Instantiate(Resources.Load<ActivityBegin>(m_BeginPrefabName), _beginRoot);
                m_begin.Inflate(m_activityNode, m_language);
            }
            if (m_hasEnd && _endRoot)
            {
                m_end = Instantiate(Resources.Load<ActivityEnd>(m_EndPrefabName), _endRoot);
            }
        }

        internal async Task LoadImage(Language language, string key, RawImage imageComponent)
        {
            string imageName = StringUtils.CleanFromWezit(GetKeyNodeForLanguage(language, key));
            if (!string.IsNullOrEmpty(imageName))
            {
                imageName = imageName.Replace("wzasset://", "");
                WezitAssets.Asset asset = AssetsLoader.GetAssetById(imageName);
                await StartCoroutine(Utils.ImageUtils.SetImage(imageComponent,
                                                         asset.GetAssetSourceByTransformation(WezitSourceTransformation.default_base),
                                                         asset.GetAssetMimeTypeByTransformation(WezitSourceTransformation.default_base),
                                                         true));
            }
        }

        internal async Task LoadImage(string imageName, RawImage imageComponent)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                imageName = imageName.Replace("wzasset://", "");
                WezitAssets.Asset asset = AssetsLoader.GetAssetById(imageName);
                await StartCoroutine(Utils.ImageUtils.SetImage(imageComponent,
                                                         asset.GetAssetSourceByTransformation(WezitSourceTransformation.default_base),
                                                         asset.GetAssetMimeTypeByTransformation(WezitSourceTransformation.default_base),
                                                         true));
            }
        }

        internal async Task GetTextureForKey(Language language, string key, System.Action<Texture2D> result)
        {
            string imageName = StringUtils.CleanFromWezit(GetKeyNodeForLanguage(language, key));
            if (!string.IsNullOrEmpty(imageName))
            {
                imageName = imageName.Replace("wzasset://", "");
                WezitAssets.Asset asset = AssetsLoader.GetAssetById(imageName);
                await StartCoroutine(TextureAndSpriteUtils.GetTextureFromSource(
                                                         asset.GetAssetSourceByTransformation(WezitSourceTransformation.default_base),
                                                         result));
            }
        }

        internal JSONNode GetKeyNodeForLanguage(Language language, string key)
        {
            JSONNode keyNode = m_activityNode[language.ToString()][key];
            if (keyNode == null)
            {
                keyNode = m_activityNode["default"][key];
            }
            return keyNode;
        }
        #endregion
        #endregion
    }
}
