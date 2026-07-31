using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Graphic), typeof(AspectRatioFitter))]
public class ImageAspectRatioSetter : MonoBehaviour
{
    private Graphic m_graphic;
    private AspectRatioFitter m_aspectRatioFitter;
    private Texture m_currentTexture;

    private void OnEnable() 
    {
        m_graphic = GetComponent<Graphic>();
        m_aspectRatioFitter = GetComponent<AspectRatioFitter>();
        if (m_graphic != null)
        {
            if (m_currentTexture != m_graphic.mainTexture && m_graphic.mainTexture != null)
            {
                m_currentTexture = m_graphic.mainTexture;
                float newRatio = (float)m_currentTexture.width / m_currentTexture.height;
                if (m_aspectRatioFitter != null)
                {
                    m_aspectRatioFitter.aspectRatio = newRatio;
                }
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_graphic != null)
        {
            if (m_currentTexture != m_graphic.mainTexture && m_graphic.mainTexture != null)
            {
                m_currentTexture = m_graphic.mainTexture;
                float newRatio = (float)m_currentTexture.width / m_currentTexture.height;

                if (m_aspectRatioFitter != null)
                {
                    m_aspectRatioFitter.aspectRatio = newRatio;
                }
            }
        }
    }

    public void InitSetter()
    {
        if (m_graphic != null)
        {
            if (m_currentTexture != m_graphic.mainTexture && m_graphic.mainTexture != null)
            {
                m_currentTexture = m_graphic.mainTexture;
                float newRatio = (float)m_currentTexture.width / m_currentTexture.height;

                if (m_aspectRatioFitter != null)
                {
                    m_aspectRatioFitter.aspectRatio = newRatio;
                }
            }
        }
    }
}
