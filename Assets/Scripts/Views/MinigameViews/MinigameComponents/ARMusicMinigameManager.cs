using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using Wezit;

public class ARMusicMinigameManager : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Camera _arCamera;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Transform _arGameItemsRoot;
    [SerializeField] private ARGameItem _arGameItemPrefab;
    [Header("UI")]
    [SerializeField] private ARMusicGameCollectedNote _collectedNotePrefab;
    [SerializeField] private Transform _collectedNotesRoot;
    #endregion

    #region Private
    private float m_spawnRate;
    private float m_lifetime;
    private float m_radius;
    private float m_scale;

    private bool m_initialized;

    private bool m_spawnItems;
    private float m_previousAngle;

    private List<Poi> m_itemsPois;
    private List<ARMusicGameCollectedNote> m_collectedNotes = new List<ARMusicGameCollectedNote>();
    #endregion
    #endregion

    #region Properties
    public UnityEvent<ARGameItem> PlayerHitObject = new UnityEvent<ARGameItem>();

    public List<ARMusicGameCollectedNote> CollectedNotes { get => m_collectedNotes; }
    #endregion

    #region Methods
    #region Monobehaviours
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && m_initialized)
        {
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
    public void Inflate(int numberOfItems, float spawnRate, float lifetime, float radius, float scale, List<Poi> itemsPois)
    {
        m_itemsPois = itemsPois;

        m_spawnRate = spawnRate;
        m_lifetime = lifetime;
        m_radius = radius;
        m_scale = scale;
        m_collectedNotes.Clear();

        m_initialized = true;

        for (int i = 1; i < _collectedNotesRoot.childCount; i++)
        {
            Destroy(_collectedNotesRoot.GetChild(i).gameObject);
        }
    }

    public void StartGame()
    {
        m_spawnItems = true;
        StartCoroutine(SpawningItemsCoroutine());
    }

    public void EndGame()
    {
        m_spawnItems = false;

        foreach (Transform child in _arGameItemsRoot)
        {
            Destroy(child.gameObject);
        }
    }

    public void GoodObjectHit(ARGameItem arGameItem)
    {
        m_collectedNotes.Add(Instantiate(_collectedNotePrefab, _collectedNotesRoot).Inflate(arGameItem.Poi));
    }
    #endregion

    #region Private
    private IEnumerator SpawningItemsCoroutine()
    {
        while (m_spawnItems)
        {
            yield return new WaitForSeconds(m_spawnRate);
            int randomPoiIndex = Random.Range(0, m_itemsPois.Count);
            float angle = Random.Range(0, 359);

            while (Mathf.Abs(angle - m_previousAngle) < 10)
            {
                angle = Random.Range(0, 359);
            }

            Instantiate(_arGameItemPrefab, _arGameItemsRoot).Inflate(true, m_itemsPois[randomPoiIndex], m_radius, angle, m_scale, m_lifetime, _arCamera);
            m_previousAngle = angle;
        }
    }
    #endregion
    #endregion
}
