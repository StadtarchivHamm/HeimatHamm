using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class DragDropMinigameView : MinigameView
{
	#region Fields
	#region Serialize Fields
	[SerializeField] private RawImage _dropAreaBackground;
	[SerializeField] private Transform _dragItemsRoot;
	[SerializeField] private DragItem _dragItemPrefab;
	[SerializeField] private List<DropZone> _dropZones;
	[SerializeField] private List<DragZone> _dragZones;
	#endregion Serialize Fields

	#region Public Variables
	#endregion Public Variables

	#region Private m_Variables
	#endregion Private m_Variables
	private int m_numberOfItems;
	public int m_numberOfDroppedItems;
	#endregion Fields

	#region Properties
	#endregion Properties

	#region Methods
	#region Public
	#endregion Public
    #region Private
    protected override async void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

		ImageUtils.LoadImage(_dropAreaBackground, this, m_minigamePoi, fillParent:false);

		await m_minigamePoi.AreRelationsSet();
		m_numberOfItems = m_minigamePoi.RefPictureRelations.Count;
        List<int> availableDragZones = new List<int>() { 0, 1, 2, 3 };

        for (int i = 0; i < m_numberOfItems; i++)
		{
			DragItem dragItem = Instantiate(_dragItemPrefab, _dragItemsRoot);
			
			int randomDragZoneIndex = Random.Range(0, availableDragZones.Count);
			dragItem.Inflate(_dragZones[availableDragZones[randomDragZoneIndex]], i);
			availableDragZones.RemoveAt(randomDragZoneIndex);

            ImageUtils.LoadRefImage(dragItem.DragItemImage, this, m_minigamePoi, i, fillParent:false);
		}
    }

    protected override void ResetViewContent()
    {
        base.ResetViewContent();

		foreach (Transform child in _dragItemsRoot)
		{
			Destroy(child.gameObject);
		}
		m_numberOfDroppedItems = 0;
    }



    protected override void AddListeners()
	{
		base.AddListeners();

		foreach (DropZone dropZone in _dropZones)
		{
			dropZone.ItemDropped.AddListener(OnItemDropped);
			dropZone.ItemRemoved.AddListener(OnItemRemoved);
		}
	}

	protected override void RemoveListeners()
	{
		base.RemoveListeners();

        foreach (DropZone dropZone in _dropZones)
        {
            dropZone.ItemDropped.RemoveListener(OnItemDropped);
            dropZone.ItemRemoved.RemoveListener(OnItemRemoved);
        }
    }
	#endregion Private
	private void OnItemDropped()
	{
		m_numberOfDroppedItems++;

		if (m_numberOfDroppedItems >= m_numberOfItems)
		{
			OnActivityOver();
        }
	}

	private void OnItemRemoved()
	{
		m_numberOfDroppedItems--;
	}
	#region Internals
	#endregion Internals
	#endregion Methods
}