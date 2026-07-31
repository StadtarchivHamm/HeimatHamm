using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlidingPuzzlePiece : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Material _slidingPuzzleMaterial;
    [SerializeField] private Image _pieceImage;
    [SerializeField] private GameObject _pieceImageShadow;
    [SerializeField] private Button _button;
    #endregion
    #region Private
    private Material m_slidingPuzzleMaterialInstance;

    private int m_pieceIndex;
    public int m_siblingIndex;
    #endregion
    #endregion

    #region Properties
    public UnityEvent<int, int> PieceClicked = new UnityEvent<int, int>();
    public bool IsEmptyPiece;
    public bool IsInRightSpot;
    public int SiblingIndex {  get { return m_siblingIndex; } }
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public SlidingPuzzlePiece Inflate(Texture2D image, int sizeOfSquare, int posX, int posY, int childIndex, bool imageEnabled)
    {
        _button.onClick.AddListener(OnButtonClick);

        m_siblingIndex = m_pieceIndex = childIndex;
        name = "Piece #" + m_siblingIndex;

        _pieceImage.enabled = imageEnabled;
        _pieceImageShadow.SetActive(imageEnabled);
        IsEmptyPiece = !imageEnabled;

        m_slidingPuzzleMaterialInstance = new Material(_slidingPuzzleMaterial);
        _pieceImage.material = m_slidingPuzzleMaterialInstance;
        m_slidingPuzzleMaterialInstance.SetTexture("_TaquinSprite", image);
        m_slidingPuzzleMaterialInstance.SetVector("_Tiling", 1f / sizeOfSquare * Vector2.one);

        float offsetX = posX / (float)sizeOfSquare;
        float offsetY = (sizeOfSquare - posY) / (float)sizeOfSquare - 1f / sizeOfSquare;
        m_slidingPuzzleMaterialInstance.SetVector("_Offset", new Vector2(offsetX, offsetY));

        return this;
    }

    public void ToggleImage(bool isOn)
    {
        _pieceImage.enabled = isOn;
    }

    public void SetIndex(int index)
    {
        transform.SetSiblingIndex(index);
        m_siblingIndex = index;
        IsInRightSpot = index == m_pieceIndex;
    }
    #endregion
    #region Private
    private void OnButtonClick()
    {
        PieceClicked?.Invoke(m_pieceIndex, m_siblingIndex);
    }
    #endregion
    #endregion
}
