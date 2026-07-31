using SimpleJSON;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Wezit;

public class SlidingPuzzleActivity : Activity
{
    #region Fields
    #region SerializeFields
    [SerializeField] private SlidingPuzzle _slidingPuzzle;
    [SerializeField] private RawImage _completedImage;
    #endregion
    #region Private
    private int m_numberOfPieces;
    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public override async void Inflate(JSONNode activityNode, Language language = Language.none)
    {
        base.Inflate(activityNode, language);
        _completedImage.gameObject.SetActive(false);

        _slidingPuzzle.PuzzleSolved.RemoveListener(OnPuzzleSolved);
        _slidingPuzzle.PuzzleSolved.AddListener(OnPuzzleSolved);
        m_numberOfPieces = GetKeyNodeForLanguage(language, "template.activity.taquin.pieces.number");
        await GetTextureForKey(m_language, "template.activity.taquin.image", LoadSlidingPuzzle);
        await LoadImage(m_language, "template.activity.taquin.image", _completedImage);
    }

    public override void StartActivity()
    {
        base.StartActivity();
        _slidingPuzzle.StartGame();
    }
    #endregion
    #region Private
    private void LoadSlidingPuzzle(Texture2D texture)
    {
        _slidingPuzzle.Inflate(texture, (int)Mathf.Floor(Mathf.Sqrt(m_numberOfPieces)));
    }

    private void OnPuzzleSolved()
    {
        _completedImage.gameObject.SetActive(true);
        ActivityOver?.Invoke();
    }
    #endregion
    #endregion
}
