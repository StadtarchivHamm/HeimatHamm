using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Reflection;
using UnityEditor;

[ExecuteInEditMode]
[AddComponentMenu("UI/TextWithBackground")]
public class TextWithBackground : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [Header("Text")]
    [TextArea()]
    [SerializeField] private string _text = "";
    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private Color _fontColor = Color.black;
    [SerializeField] private float _fontSize = 32;
    [Header("Background")]
    [SerializeField] private Color _backgroundColor = Color.black;
    [SerializeField] private Vector2 _verticalPadding = Vector2.zero;
    [SerializeField] private Vector2 _horizontalPadding = Vector2.zero;
    [Space]
    [SerializeField] private bool _updateStyle;
    #endregion
    #region Private
    [SerializeField] private TextMeshProUGUI m_backgroundText;
    [SerializeField] private TextMeshProUGUI m_text;

    private bool m_initialized;
    #endregion
    #endregion

    #region Properties
    public string text
    {
        get => _text;
        set {
            _text = value;
            UpdateText();
        }
    }
    #endregion

    #region Methods
    #region Monobehaviours
#if UNITY_EDITOR
    // Add a menu item to create custom GameObjects.
    // Priority 1 ensures it is grouped with the other menu items of the same kind
    // and propagated to the hierarchy dropdown and hierarch context menus.
    [MenuItem("GameObject/UI/Text With Background", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("TextWithBG");
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        go.AddComponent<TextWithBackground>();
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif

    // Start is called before the first frame update
    void Start()
    {
        if (!m_initialized)
        {
            Init();
        }
    }

#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (_updateStyle)
        {
            _updateStyle = false;
            UpdateText();
        }
    }
#endif
#endregion
    #region Public

    #endregion
    #region Private
    private void Init()
    {
        if (gameObject.GetComponent<TextMeshProUGUI>() != null)
        {
            m_backgroundText = gameObject.GetComponent<TextMeshProUGUI>();
            foreach (Transform child in transform)
            {
                if (child.GetComponent<TextMeshProUGUI>() != null)
                {
                    m_text = child.GetComponent<TextMeshProUGUI>();
                    break;
                }
            }

            m_initialized = true;
            return;
        }

        m_backgroundText = gameObject.AddComponent<TextMeshProUGUI>();

        GameObject childText = new GameObject("Text");
        m_text = childText.AddComponent<TextMeshProUGUI>();

        childText.transform.SetParent(transform);
        childText.transform.localScale = Vector3.one;
        RectTransform childRectTransform = childText.GetComponent<RectTransform>();
        childRectTransform.anchorMin = Vector2.zero;
        childRectTransform.anchorMax = Vector2.one;
        childRectTransform.offsetMin = Vector2.zero;
        childRectTransform.offsetMax = Vector2.zero;

        m_initialized = true;
    }

    private void UpdateText()
    {
        if (!m_initialized)
        {
            Init();
        }
        string color = "<mark=#" + ColorUtility.ToHtmlStringRGBA(_backgroundColor);
        string padding = " padding=" + '"' + _horizontalPadding.x + "," + _horizontalPadding.y + "," + _verticalPadding.x + "," + _verticalPadding.y + "\">";
        m_backgroundText.text = color + padding + _text + "</mark>";


        m_text.fontSize = m_backgroundText.fontSize = _fontSize;
        m_text.font = m_backgroundText.font = _font;
        m_text.color = m_backgroundText.color = _fontColor;

        m_text.characterSpacing = m_backgroundText.characterSpacing;
        m_text.lineSpacing = m_backgroundText.lineSpacing;
        m_text.lineSpacingAdjustment = m_backgroundText.lineSpacingAdjustment;
        m_text.paragraphSpacing = m_backgroundText.paragraphSpacing;
        m_text.wordSpacing = m_backgroundText.wordSpacing;
        m_text.alignment = m_backgroundText.alignment;
        m_text.fontStyle = m_backgroundText.fontStyle;
        m_text.textStyle = m_backgroundText.textStyle;

        m_text.text = _text;
    }
    #endregion
    #endregion
}
