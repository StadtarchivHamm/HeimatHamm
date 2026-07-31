using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Wezit;

public class ARMinigameManager : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Camera _arCamera;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Transform _arGameItemsRoot;
    [SerializeField] private ARGameItem _arGameItemPrefab;
    [Header("UI")]
    [SerializeField] private Image _tool;
    [SerializeField] private AudioSource _toolAudioSource;
    [Space]
    [SerializeField] private RawImage _itemsThumbnail;
    [SerializeField] private TextMeshProUGUI _remainingItems;
    [Space]
    [SerializeField] private GameObject _lifePrefab;
    [SerializeField] private GameObject _lives;
    #endregion

    #region Private
    private int m_numberOfItems;
    private float m_spawnRate;
    private float m_chanceOfBadItem;
    private float m_lifetime;
    private float m_radius;

    private bool m_initialized;

    private bool m_spawnItems;
    private List<Sprite> m_toolSprites;
    private AudioClip m_toolSoundEffect;
    private List<RawImage> m_remainingLives = new List<RawImage>();

    private AudioClip m_goodItemSoundEffect;
    private AudioClip m_badItemSoundEffect;

    #endregion
    #endregion

    #region Properties
    public UnityEvent<ARGameItem> PlayerHitObject = new UnityEvent<ARGameItem>();
    #endregion

    #region Methods
    #region Monobehaviours
    private void Update()
    {
        if (!m_initialized)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && m_initialized)
        {
            StartCoroutine(AnimateTool(0.1f));

            Ray ray = _arCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 15,  _layerMask))
            {
                ARGameItem aRGameItem = hit.transform.GetComponent<ARGameItem>();
                aRGameItem.Hit();
                PlayerHitObject?.Invoke(aRGameItem);
                Debug.Log("Did Hit " + hit.transform.name);
            }
        }
    }
    #endregion

    #region Public
    public void Inflate(int numberOfItems, float spawnRate, float chanceOfBadItem, int numberOfLives, float lifetime, float radius, List<Sprite> toolSprites, List<Sprite> goodItemSprites, List<Sprite> badItemSprites, AudioClip toolSoundEffect = null, AudioClip goodItemSoundEffect = null, AudioClip badItemSoundEffect = null)
    {
        _lives.SetActive(chanceOfBadItem > 0);

        m_numberOfItems = numberOfItems;
        _remainingItems.text = m_numberOfItems.ToString();
        m_spawnRate = spawnRate;
        m_chanceOfBadItem = chanceOfBadItem;
        _itemsThumbnail.texture = goodItemSprites[0].texture;

        m_lifetime = lifetime;
        m_radius = radius;

        _toolAudioSource.clip = m_toolSoundEffect = toolSoundEffect;
        m_goodItemSoundEffect = goodItemSoundEffect;
        m_badItemSoundEffect = badItemSoundEffect;

        foreach (Transform lifeSprite in _lives.transform)
        {
            Destroy(lifeSprite.gameObject);
        }
        m_remainingLives.Clear();

        if (chanceOfBadItem > 0 && badItemSprites?.Count > 0)
        {
            for (int i = 0; i < numberOfLives; i++)
            {
                m_remainingLives.Add(Instantiate(_lifePrefab, _lives.transform).transform.GetChild(0).GetComponent<RawImage>());
                m_remainingLives[i].texture = badItemSprites[0].texture;
            }
        }

        m_toolSprites = toolSprites;
        _tool.sprite = toolSprites[0];

        m_initialized = true;
    }

    public void StartGame(List<Sprite> goodItemSprites, List<Sprite> badItemSprites)
    {
        m_spawnItems = true;
        StartCoroutine(SpawningItemsCoroutine(goodItemSprites, badItemSprites));
    }

    public void EndGame()
    {
        m_spawnItems = false;
        m_initialized = false;

        foreach (Transform child in _arGameItemsRoot)
        {
            Destroy(child.gameObject);
        }
    }

    public void BadObjectHit(int remainingLives)
    {
        m_remainingLives[remainingLives].enabled = false;
    }

    public void GoodObjectHit(int remainingItems)
    {
        _remainingItems.text = remainingItems.ToString();
    }
    #endregion

    #region Private
    private IEnumerator SpawningItemsCoroutine(List<Sprite> goodItemSprites, List<Sprite> badItemsSprites)
    {
        while (m_spawnItems)
        {
            yield return new WaitForSeconds(m_spawnRate);
            if (m_chanceOfBadItem != 0)
            {
                bool isGoodItem = Random.Range(0, 1f) > m_chanceOfBadItem;
                Instantiate(_arGameItemPrefab, _arGameItemsRoot).Inflate(isGoodItem, isGoodItem ? goodItemSprites : badItemsSprites, m_radius, m_lifetime, _arCamera, isGoodItem ? m_goodItemSoundEffect : m_badItemSoundEffect);
            }
            else
            {
                Instantiate(_arGameItemPrefab, _arGameItemsRoot).Inflate(true, goodItemSprites, m_radius, m_lifetime, _arCamera, m_goodItemSoundEffect);
            }
        }
    }

    private IEnumerator AnimateTool(float frameRate)
    {
        if (m_toolSoundEffect != null)
        {
            _toolAudioSource.Play();
        }
        foreach (Sprite toolSprite in m_toolSprites)
        {
            _tool.sprite = toolSprite;
            yield return new WaitForSeconds(frameRate);
        }
        _tool.sprite = m_toolSprites[0];
    }
    #endregion
    #endregion
}
