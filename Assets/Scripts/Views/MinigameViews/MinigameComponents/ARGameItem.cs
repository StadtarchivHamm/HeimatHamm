using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARGameItem : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Animator _spriteAnimator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private int _referenceHeight;
    [SerializeField] private float _referenceScale;
    [SerializeField] private Billboard _billboard;
    [SerializeField] private int _goodItemLayer;
    [SerializeField] private int _badItemLayer;
    #endregion
    #region Private
    private bool m_isPositive;
    private List<Sprite> m_sprites;
    private int m_currentSpriteIndex;
    private AudioClip m_soundEffect;
    private Wezit.Poi m_poi;
    #endregion
    #endregion

    #region Properties
    public bool IsPositive {  get { return m_isPositive; } }
    public AudioClip SoundEffect {  get { return m_soundEffect; } }
    public Wezit.Poi Poi {  get { return m_poi; } }
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public void Inflate(bool isPositive, List<Sprite> sprites, float radius, float lifetime, Camera camera, AudioClip soundEffect = null)
    {
        gameObject.layer = isPositive ? _goodItemLayer : _badItemLayer;
        name = isPositive ? "Good item" : "Bad item";
        m_isPositive = isPositive;
        m_sprites = sprites;
        _spriteRenderer.sprite = m_sprites[0];
        _spriteRenderer.transform.localScale = _referenceScale * _referenceHeight / m_sprites[0].texture.height * Vector3.one;

        _billboard.Inflate(camera);

        _audioSource.clip = m_soundEffect = soundEffect;

        float angle = Random.Range(0, 359);
        float newRadius = radius * Random.Range(0.9f, 1.1f);
        transform.localPosition = new Vector3(newRadius * Mathf.Cos(angle), transform.localPosition.y, newRadius * Mathf.Sin(angle));

        StartCoroutine(LifeCoroutine(lifetime));
        StartCoroutine(FloatUpAndDown());

        if (sprites.Count > 1)
        {
            StartCoroutine(SpriteAnimation(0.1f));
        }
    }

    public async void Inflate(bool isPositive, Wezit.Poi poi, float radius, float angle, float scale, float lifetime, Camera camera)
    {
        m_poi = poi;
        _spriteRenderer.enabled = false;

        gameObject.layer = isPositive ? _goodItemLayer : _badItemLayer;
        name = isPositive ? "Good item" : "Bad item";
        m_isPositive = isPositive;

        _billboard.Inflate(camera);

        float newRadius = radius * Random.Range(0.9f, 1.1f);
        transform.localPosition = new Vector3(newRadius * Mathf.Cos(angle), transform.localPosition.y, newRadius * Mathf.Sin(angle));
        transform.localScale = scale * Vector3.one;

        await poi.AreRelationsSet();
        if (poi.ShowPictureRelations?.Count > 0)
        {
            StartCoroutine(TextureAndSpriteUtils.GetSpriteFromSource(poi.ShowPictureRelations[Random.Range(0, poi.ShowPictureRelations.Count)].GetAssetSourceByTransformation("default"), OnSpriteDownloaded));
        }
        else
        {
            Debug.LogWarning("No showPicture relation for POI " + poi.title);
        }
        _audioSource.clip = m_soundEffect = await AudioUtils.GetAudioClip(poi);

        StartCoroutine(LifeCoroutine(lifetime));
        StartCoroutine(FloatUpAndDown());
    }

    public void Hit()
    {
        if (m_soundEffect != null)
        {
            _audioSource.Play();
        }
        _spriteAnimator.enabled = true;
        _spriteAnimator.SetTrigger("Die");
    }

    // Called by the animator
    public void DestroyItem()
    {
        Destroy(gameObject);
    }
    #endregion
    #region Private
    private IEnumerator LifeCoroutine(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        _spriteAnimator.enabled = true;
        _spriteAnimator.SetTrigger("Disappear");
    }

    private IEnumerator SpriteAnimation(float frameRate)
    {
        while (true)
        {
            yield return new WaitForSeconds(frameRate);
            m_currentSpriteIndex = (m_currentSpriteIndex + 1) % m_sprites.Count;
            _spriteRenderer.sprite = m_sprites[m_currentSpriteIndex];
        }
    }

    private IEnumerator FloatUpAndDown()
    {
        float timer = 0;
        Vector3 localPosition = transform.localPosition;
        while (true)
        {
            timer += Time.deltaTime;
            localPosition.y = .1f * Mathf.Sin(timer * 1.5f);
            transform.localPosition = localPosition;
            yield return null;
        }
    }

    private void OnSpriteDownloaded(Sprite sprite)
    {
        _spriteRenderer.enabled = true;
        _spriteRenderer.sprite = sprite;
        _spriteRenderer.transform.localScale = _referenceScale * _referenceHeight / sprite.texture.height * Vector3.one;
    }
    #endregion
    #endregion
}
