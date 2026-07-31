using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace Wezit
{
    public class TouchActivity : Activity
    {
        #region Fields
        #region SerializeFields
        [SerializeField] private RawImage _background;
        [SerializeField] private TouchActivityItem _touchPointPrefab;
        [SerializeField] private RectTransform _touchPointsRoot;
        [SerializeField] private GameObject _findingLayerRoot;
        [SerializeField] private Image _findingLayerBackground;
        #endregion
        #region Private
        private bool m_backgroundLoaded;
        public int m_numberOfTouchItems;
        public int m_numberOfTouchItemsTouched;
        #endregion
        #endregion

        #region Properties
        public UnityEvent<bool> QuizOver = new UnityEvent<bool>();
        #endregion

        #region Methods
        #region Monobehaviour
        private void OnDisable()
        {
            _findingLayerRoot.SetActive(false);
        }
        #endregion

        #region Public
        public override void Inflate(JSONNode activityNode, Language language)
        {
            base.Inflate(activityNode, language);

            _findingLayerRoot.SetActive(false);
            if (m_activityNodeInStateLanguage["template.activity.general.instruction.text.content"].ToString().Contains("overlay"))
            {
                StartCoroutine(InitFindingLayer());
            }

            if (m_activityNodeInStateLanguage["template.activity.select.items"].Count == 0)
            {
                Debug.LogError("There are no points in the touch activity, which means it can't be initialized");
                return;
            }


            foreach (Transform child in _touchPointsRoot)
            {
                Destroy(child.gameObject);
            }

            m_backgroundLoaded = false;
            m_numberOfTouchItems = 0;
            m_numberOfTouchItemsTouched = 0;

            foreach (JSONNode item in m_activityNodeInStateLanguage["template.activity.select.items"])
            {
                TouchItemModel touchItem = new TouchItemModel(item["color"],
                                                     item["point"],
                                                     item["response.status"],
                                                     item["response.validation.status"],
                                                     item["response.validation.image"],
                                                     item["response.validation.title.text.content"],
                                                     item["response.validation.description.text.content"]);
                Instantiate(_touchPointPrefab, _touchPointsRoot).Inflate(touchItem, _touchPointsRoot.sizeDelta).TouchItemTouched.AddListener(OnItemTouched);

                if (!m_backgroundLoaded)
                {
                    LoadImage(touchItem.point.map.pid, _background);
                    m_backgroundLoaded = true;
                }

                m_numberOfTouchItems++;
            }
        }
        #endregion

        #region Internal
        #endregion

        #region Private
        private void OnItemTouched()
        {
            m_numberOfTouchItemsTouched++;

            if (m_numberOfTouchItemsTouched >= m_numberOfTouchItems)
            {
                _findingLayerRoot.SetActive(false);

                ActivityOver?.Invoke();
            }
        }

        private IEnumerator InitFindingLayer()
        {
            yield return null;
            _findingLayerRoot.SetActive(true);

            if (_findingLayerRoot.activeInHierarchy)
            {
                Vector3[] corners = new Vector3[4];
                _touchPointsRoot.parent.GetComponent<RectTransform>().GetWorldCorners(corners);
                Vector2 sizeDelta = new Vector2(corners[2].x - corners[0].x, corners[1].y - corners[0].y) / GetComponentInParent<Canvas>().transform.localScale.x;
                _findingLayerRoot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                _findingLayerRoot.GetComponent<RectTransform>().sizeDelta = _touchPointsRoot.sizeDelta = sizeDelta;
            }

            if (m_activityNodeInStateLanguage["template.activity.general.instruction.text.content"].ToString().Contains("|"))
            {
                string[] splitContent = m_activityNodeInStateLanguage["template.activity.general.instruction.text.content"].ToString().Split('|');
                if (splitContent.Length > 1)
                {
                    _findingLayerRoot.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = StringUtils.GetStringAsFloat(splitContent[1].Replace("\"", ""), 100) * Vector2.one;
                }

            }
        }
        #endregion
        #endregion
    }
}
