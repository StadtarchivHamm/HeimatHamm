using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler
{

    private TextMeshProUGUI m_textMeshPro = null;
    internal TextMeshProUGUI TextMeshPro
    {
        get
        {
            if (m_textMeshPro == null)
                m_textMeshPro = GetComponent<TextMeshProUGUI>();
            return m_textMeshPro;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 position;
#if UNITY_EDITOR
        position = Input.mousePosition;

#else
        position = Input.GetTouch(0).position;
#endif
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(TextMeshPro, position, null);
        if (linkIndex != -1)
        { // was a link clicked?
            TMP_LinkInfo linkInfo = TextMeshPro.textInfo.linkInfo[linkIndex];

            // open the link id as a url, which is the metadata we added in the text field
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}