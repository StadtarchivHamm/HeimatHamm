using SimpleJSON;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wezit
{
    public class ScratchAndReveal : Activity
    {
        #region Fields
        #region SerializeFields
        [SerializeField] private RawImage _backgroundImage = null;
        [SerializeField] private RawImage _foregroundImage = null;
        [SerializeField] private GraphicFader _foregroundFader = null;
        [SerializeField] private TextMeshProUGUI _progressText = null;
        #endregion
        #region Private
        private RectTransform m_RectTransform;
        private float m_RectWidth;
        private float m_RectHeight;
        private Camera m_MainCamera;
        private bool m_StopGame;

        private Texture2D m_ForegroundTexture;
        private Texture2D m_MaskTexture;
        private float m_TexWidth;
        private float m_TexHeight;
        private int m_RevealedPixels;
        private float m_PixelsToReveal;
        private float m_CompletionRate;

        public int m_Radius = 5;
        private Color m_Transparent = new Color(0, 0, 0, 0);
        #endregion
        #endregion

        #region Properties
        #endregion

        #region Methods
        #region Monobehaviour
        #endregion

        #region Public
        public override async void Inflate(JSONNode activityNode, Language language)
        {
            m_RectTransform = gameObject.GetComponent<RectTransform>();
            m_RectWidth = m_RectTransform.rect.width;
            m_RectHeight = m_RectTransform.rect.height;
            m_MainCamera = Camera.main;

            _progressText.text = string.Format("Completion: {0}%", m_CompletionRate.ToString("0.00"));

            base.Inflate(activityNode, language);
            await LoadImage(language, "template.activity.scratch.background.image", _backgroundImage);
            await GetTextureForKey(language, "template.activity.scratch.foreground.image", ApplyForegroundTexture);
            await GetTextureForKey(language, "template.activity.scratch.mask.image", ApplyMaskTexture);

            StartCoroutine(Scratch());
        }
        #endregion
        #region Internal
        #endregion
        #region Private
        private IEnumerator Scratch()
        {
            while (!m_StopGame)
            {
                if (Input.GetMouseButton(0))
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, Input.mousePosition, m_MainCamera, out Vector2 m_MousePos);

                    // Calculate distance from lower left corner of the rect
                    m_MousePos.x = m_MousePos.x + m_RectWidth / 2;
                    m_MousePos.y = m_MousePos.y + m_RectHeight / 2;

                    // Then erase pixels in a circle around the mouse position
                    ColorInCircle((int)(m_MousePos.x / m_RectWidth * m_TexWidth), (int)(m_MousePos.y / m_RectHeight * m_TexHeight), m_Radius, m_ForegroundTexture.width, m_ForegroundTexture.height);

                    m_CompletionRate = m_RevealedPixels / m_PixelsToReveal * 100f;
                    _progressText.text = string.Format("Completion: {0}%", m_CompletionRate.ToString("0.00"));
                }
                yield return null;

                if (Input.GetMouseButtonUp(0))
                {
                    m_CompletionRate = m_RevealedPixels / m_PixelsToReveal * 100f;
                    _progressText.text = string.Format("Completion: {0}%", m_CompletionRate.ToString("0.00"));
                    if (m_CompletionRate > 80)
                    {
                        m_StopGame = true;
                        Handheld.Vibrate();
                    }
                }
            }
            m_StopGame = false;
            if(_foregroundFader != null)
            {
                _foregroundFader.StartFadingFromInit();
            }
            else
            {
                _foregroundImage.gameObject.SetActive(false);
            }
        }

        private void ColorInCircle(int centerX, int centerY, int radius, int imageWidth, int imageHeight)
        {
            for (int x = -radius; x < radius; x++)
            {
                int height = (int)Mathf.Sqrt(radius * radius - x * x);

                for (int y = -height; y < height; y++)
                {
                    if (centerX + x < 0 || centerX + x > imageWidth || centerY + y < 0 || centerY + y > imageHeight) continue;
                    if(m_ForegroundTexture.GetPixel(centerX + x, centerY + y) != m_Transparent)
                    {
                        if (m_MaskTexture.GetPixel(centerX + x, centerY + y) == Color.black) m_RevealedPixels++;
                        m_ForegroundTexture.SetPixel(centerX + x, centerY + y, m_Transparent);
                    }
                }
            }
            m_ForegroundTexture.Apply();
        }

        private void ApplyForegroundTexture(Texture2D texture)
        {
            m_ForegroundTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            m_ForegroundTexture.SetPixels(texture.GetPixels());
            m_ForegroundTexture.Apply();
            _foregroundImage.texture = m_ForegroundTexture;
            m_TexWidth = m_ForegroundTexture.width;
            m_TexHeight = m_ForegroundTexture.height;

            // Check if rect width and height have changed
            m_RectWidth = m_RectTransform.rect.width;
            m_RectHeight = m_RectTransform.rect.height;

            float ratio = m_TexWidth / m_TexHeight;
            Debug.LogError(Screen.width / (float)Screen.height);
            m_RectWidth = ratio * m_RectHeight;
            m_RectTransform.sizeDelta = new Vector2(m_RectWidth, m_RectHeight);
        }

        private void ApplyMaskTexture(Texture2D texture)
        {
            m_MaskTexture = texture;
            if (texture == null)
            {
                m_MaskTexture = new Texture2D(m_ForegroundTexture.width, m_ForegroundTexture.height, TextureFormat.ARGB32, false);
                Color[] pixels = m_MaskTexture.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.black;
                }
                m_MaskTexture.SetPixels(pixels);
                m_MaskTexture.Apply();
            }
            for (int x = 0; x < m_MaskTexture.width; x++)
            {
                for (int y = 0; y < m_MaskTexture.height; y++)
                {
                    if(m_MaskTexture.GetPixel(x, y) == Color.black)
                    {
                        m_PixelsToReveal++;
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}
