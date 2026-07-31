using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SlidingPuzzle : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private bool _initOnStart;
    [SerializeField] private Texture2D _slidingPuzzleImage;
    [SerializeField] private int _sizeOfSquare;
    [SerializeField] private SlidingPuzzlePiece _slidingPuzzlePiecePrefab;
    [SerializeField] private RectTransform _piecesRoot;
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private int _gridLayoutSpacing = 5;
    [SerializeField] private bool _showShuffling;
    [SerializeField] private GameObject _shufflingRaycastBlocker;
    [SerializeField] private int _shufflingMovesBetweenYields = 1;
    #endregion
    #region Private
    private List<SlidingPuzzlePiece> m_pieces = new List<SlidingPuzzlePiece>();
    private int m_numberOfPieces;
    private int m_numberOfPiecesInRightPlace;
    private int m_emptySpaceSiblingIndex;
    private bool m_isShuffling;
    #endregion
    #endregion

    #region Properties
    public UnityEvent PuzzleSolved = new UnityEvent();
    #endregion

    #region Methods
    #region Monobehaviours
    private void Start()
    {
        if (_initOnStart)
        {
            Init();
            StartGame();
        }
    }
    #endregion
    #region Public
    public void Inflate(Texture2D puzzleImage, int sizeOfSquare)
    {
        _slidingPuzzleImage = puzzleImage;
        _sizeOfSquare = sizeOfSquare;

        Init();
    }

    public void StartGame()
    {
        StartCoroutine(ShuffleBoardAfterDelay(1));
    }
    #endregion
    #region Private
    private void Init()
    {
        _sizeOfSquare = _sizeOfSquare == 0 ? 3 : _sizeOfSquare;
        m_numberOfPieces = (int)Mathf.Pow(_sizeOfSquare, 2);
        m_numberOfPiecesInRightPlace = 0;
        m_emptySpaceSiblingIndex = m_numberOfPieces - 1;

        foreach (Transform child in _piecesRoot)
        {
            Destroy(child.gameObject);
        }
        m_pieces.Clear();

        int childIndex = 0;
        for (int y = 0; y < _sizeOfSquare; y++)
        {
            for (int x = 0; x < _sizeOfSquare; x++)
            {
                m_pieces.Add(Instantiate(_slidingPuzzlePiecePrefab, _piecesRoot).Inflate(_slidingPuzzleImage, _sizeOfSquare, x, y, childIndex, childIndex != m_emptySpaceSiblingIndex));
                m_pieces[childIndex].PieceClicked.AddListener(OnPieceClicked);
                childIndex++;
            }
        }

        _gridLayout.cellSize = (_piecesRoot.sizeDelta.x - _gridLayoutSpacing * (_sizeOfSquare - 1)) / _sizeOfSquare * Vector2.one;
        _gridLayout.spacing = _gridLayoutSpacing * Vector2.one;
    }

    private IEnumerator ShuffleBoardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        StartCoroutine(Shuffle());
    }

    // Shuffle by sliding pieces a certain number of times.
    private IEnumerator Shuffle(int numberOfMoves = 100)
    {
        m_isShuffling = true;
        _shufflingRaycastBlocker.SetActive(true);
        int count = 0;
        int last = 0;

        while (count < numberOfMoves)
        {
            int randomPiece = Random.Range(0, m_numberOfPieces);

            // Only thing we forbid is undoing the last move.
            if (randomPiece == last) 
            { 
                continue; 
            }

            // Try surrounding spaces looking for valid move.
            if (MovePieceIfPossible(randomPiece, m_pieces[randomPiece].SiblingIndex, -_sizeOfSquare, _sizeOfSquare))
            {
                last = randomPiece;
                count++;

                if (_showShuffling && count % _shufflingMovesBetweenYields == 0)
                {
                    yield return null;
                }
            }
            else if (MovePieceIfPossible(randomPiece, m_pieces[randomPiece].SiblingIndex, _sizeOfSquare, _sizeOfSquare))
            {
                last = randomPiece;
                count++;

                if (_showShuffling && count % _shufflingMovesBetweenYields == 0)
                {
                    yield return null;
                }
            }
            else if (MovePieceIfPossible(randomPiece, m_pieces[randomPiece].SiblingIndex, -1, 0))
            {
                last = randomPiece;
                count++;

                if (_showShuffling && count % _shufflingMovesBetweenYields == 0)
                {
                    yield return null;
                }
            }
            else if (MovePieceIfPossible(randomPiece, m_pieces[randomPiece].SiblingIndex, +1, _sizeOfSquare - 1))
            {
                last = randomPiece;
                count++;

                if (_showShuffling && count % _shufflingMovesBetweenYields == 0)
                {
                    yield return null;
                }
            }
        }

        if (!_showShuffling)
        {
            yield return null;
        }

        _shufflingRaycastBlocker.SetActive(false);
        m_isShuffling = false;
    }

    private void OnPieceClicked(int pieceIndex, int siblingIndex)
    {
        // Check left cell
        if (MovePieceIfPossible(pieceIndex, siblingIndex, -1, 0))
        {
            return;
        }

        // Check right cell
        if (MovePieceIfPossible(pieceIndex, siblingIndex, 1, _sizeOfSquare - 1))
        {
            return;
        }

        // Check up cell (colCheck is set to size of square because it is not needed to check)
        if (MovePieceIfPossible(pieceIndex, siblingIndex, -_sizeOfSquare, _sizeOfSquare))
        {
            return;
        }

        // Check bottom cell
        if (MovePieceIfPossible(pieceIndex, siblingIndex, _sizeOfSquare, _sizeOfSquare))
        {
            return;
        }
    }

    // colCheck is used to stop horizontal moves wrapping.
    private bool MovePieceIfPossible(int pieceIndex, int siblingIndex, int offset, int colCheck)
    {
        if (((siblingIndex % _sizeOfSquare) != colCheck) && ((siblingIndex + offset) == m_emptySpaceSiblingIndex))
        {

            // Increment pieces in the right place counter if the moved piece is back in its original place
            if (pieceIndex == m_emptySpaceSiblingIndex)
            {
                m_numberOfPiecesInRightPlace++;

                if (m_numberOfPiecesInRightPlace == m_numberOfPieces - 1 && !m_isShuffling)
                {
                    EndGame();
                }
            }
            // Decrement it if the piece was in the right place and is moved out of it
            else if (m_pieces[pieceIndex].IsInRightSpot)
            {
                m_numberOfPiecesInRightPlace--;
            }

            m_pieces[pieceIndex].SetIndex(m_emptySpaceSiblingIndex);
            m_pieces[m_pieces.Count - 1].SetIndex(siblingIndex);
            m_emptySpaceSiblingIndex = siblingIndex;
            
            return true;
        }

        return false;
    }

    private void EndGame()
    {
        foreach (SlidingPuzzlePiece piece in m_pieces)
        {
            piece.ToggleImage(true);
        }
        _gridLayout.spacing = Vector2.zero;

        PuzzleSolved?.Invoke();
    }
    #endregion
    #endregion
}

