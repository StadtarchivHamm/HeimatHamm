using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Wezit;

public class InventoryView : BaseView
{
    #region Fields
    #region Serialize Fields
    [SerializeField] private GameObject _emptyInventoryText;
    [SerializeField] private InventoryObject _inventoryObjectPrefab;
    [SerializeField] private Transform _inventoryObjectsRoot;
    #endregion Serialize Fields

    #region Public Variables
    #endregion Public Variables

    #region Private m_Variables
    #endregion Private m_Variables
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    #endregion Public

    #region Private
    protected override void InitViewContentByLang(Language language)
    {
        base.InitViewContentByLang(language);

        bool inventoryIsEmpty = true;

        foreach (Poi hiddenObjectPoi in PlayerManager.Player.GetUnlockedHiddenObjectsPois())
        {
            inventoryIsEmpty = false;
            Instantiate(_inventoryObjectPrefab, _inventoryObjectsRoot).Inflate(hiddenObjectPoi).ObjectClicked.AddListener(OnInventoryObjectClicked);
        }
        StartCoroutine(LayoutGroupRebuilder.Rebuild(_inventoryObjectsRoot.gameObject));

        _emptyInventoryText.SetActive(inventoryIsEmpty);
    }

    protected override void ResetViewContent()
    {
        base.ResetViewContent();

        foreach (Transform hiddenObject in _inventoryObjectsRoot)
        {
            Destroy(hiddenObject.gameObject);
        }
    }

    private void OnInventoryObjectClicked(Poi poi)
    {
        PlayerManager.CurrentState.CurrentHiddenObjectPoi = poi;
        SetState(KioskState.HIDDEN_OBJECT);
    }
    #endregion Private
    #endregion Methods
}