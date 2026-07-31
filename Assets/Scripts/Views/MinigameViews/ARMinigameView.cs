using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wezit;

public class ARMinigameView : MinigameView
{
    #region Fields
    #region Serialize Fields
    #endregion Serialize Fields

    #region Public Variables
    #endregion Public Variables

    #region Private m_Variables
    #endregion Private m_Variables
    private ARMinigameManager m_arMinigameManager;

    private bool m_aRIsLoaded;

    private int m_numberOfItems;
	private float m_spawnRate;
	private float m_chanceOfBadItem;
    private float m_radius;
    private float m_lifetime;

    private int m_numberOfHitItems;
    private int m_numberOfLives = 3;

    private List<Sprite> m_toolSprites = new List<Sprite>();
    private bool m_toolSpritesInitialized;
    private AudioClip m_toolSoundEffect;

    private int m_numberOfGoodItemsSprites;
    private List<Sprite> m_goodItemsSprites = new List<Sprite>();
    private AudioClip m_goodItemSoundEffect;
    private bool m_goodItemsSpritesInitialized;


    private int m_numberOfBadItemsSprites;
    private List<Sprite> m_badItemsSprites = new List<Sprite>();
    private AudioClip m_badItemSoundEffect;
    private bool m_badItemsSpritesInitialized;
    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods
    #region Public
    #endregion Public
    public override void HideView()
    {
        if (m_aRIsLoaded)
        {
            SceneManager.UnloadSceneAsync(2);
            m_aRIsLoaded = false;
        }
        base.HideView();
    }

    #region Private
    protected async override void InitViewContentByLang(Language language)
	{
		base.InitViewContentByLang(language);

        m_numberOfLives = 3;
        m_numberOfItems = (int)StringUtils.GetStringAsFloat(m_minigamePoi.extent);
        m_spawnRate = StringUtils.GetStringAsFloat(m_minigamePoi.location);
        m_chanceOfBadItem = StringUtils.GetStringAsFloat(m_minigamePoi.spatial);
        m_lifetime = StringUtils.GetStringAsFloat(m_minigamePoi.author, 5);
        m_radius = StringUtils.GetStringAsFloat(m_minigamePoi.source, 3);

        await m_minigamePoi.AreRelationsSet();
        foreach (Relation toolRelation in m_minigamePoi.ShowPictureRelations)
        {
            StartCoroutine(TextureAndSpriteUtils.GetSpriteFromSource(toolRelation.GetAssetSourceByTransformation("default"), OnToolSpriteDownloaded));
        }
        m_toolSoundEffect = await AudioUtils.GetAudioClip(m_minigamePoi);

        m_minigamePoi.SetChildren();

        Poi goodItemPoi = m_minigamePoi.children.Find(x => x.tags.Contains("ARItemGood"));
        if (goodItemPoi == null)
        {
            Debug.LogError("No item POI in AR minigame POI " + m_minigamePoi.pid);
            return;
        }
        // Download items sprites
        await goodItemPoi.AreRelationsSet();
        m_numberOfGoodItemsSprites = goodItemPoi.ShowPictureRelations.Count;
        foreach (Relation spriteRelation in goodItemPoi.ShowPictureRelations)
        {
            StartCoroutine(TextureAndSpriteUtils.GetSpriteFromSource(spriteRelation.GetAssetSourceByTransformation("default"), OnGoodSpriteDownloaded));
        }
        m_goodItemSoundEffect = await AudioUtils.GetAudioClip(goodItemPoi);

        Poi badItemPoi = m_minigamePoi.children.Find(x => x.tags.Contains("ARItemBad"));
        if (badItemPoi != null)
        {
            await badItemPoi.AreRelationsSet();
            m_numberOfBadItemsSprites = badItemPoi.ShowPictureRelations.Count;
            foreach (Relation spriteRelation in badItemPoi.ShowPictureRelations)
            {
                StartCoroutine(TextureAndSpriteUtils.GetSpriteFromSource(spriteRelation.GetAssetSourceByTransformation("default"), OnBadSpriteDownloaded));
            }
            m_badItemSoundEffect = await AudioUtils.GetAudioClip(badItemPoi);
        }
        else
        {
            m_badItemsSpritesInitialized = true;
        }

        if (!m_aRIsLoaded)
        {
            AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
            sceneLoading.completed += OnArLoaded;
        }
        else
        {
            OnArLoaded(null);
        }
    }

    protected override void ResetViewContent()
    {
        base.ResetViewContent();

        m_toolSprites.Clear();
        m_toolSpritesInitialized = false;

        m_badItemsSprites.Clear();
        m_goodItemsSpritesInitialized = false;

        m_goodItemsSprites.Clear();
        m_badItemsSpritesInitialized = false;


        m_numberOfHitItems = 0;
    }

    private void OnToolSpriteDownloaded(Sprite toolSprite)
    {
        m_toolSprites.Add(toolSprite);

        if (m_toolSprites.Count == m_minigamePoi.ShowPictureRelations.Count)
        {
            m_toolSpritesInitialized = true;
        }
    }

    private void OnGoodSpriteDownloaded(Sprite goodSprite)
    {
        m_goodItemsSprites.Add(goodSprite);

        if (m_goodItemsSprites.Count == m_numberOfGoodItemsSprites)
        {
            m_goodItemsSpritesInitialized = true;
        }
    }

    private void OnBadSpriteDownloaded(Sprite badSprite)
    {
        m_badItemsSprites.Add(badSprite);

        if (m_badItemsSprites.Count == m_numberOfBadItemsSprites)
        {
            m_badItemsSpritesInitialized = true;
        }
    }

    private void OnArLoaded(AsyncOperation loadingAsyncOperation)
    {
        m_aRIsLoaded = true;
        m_arMinigameManager = FindFirstObjectByType<ARMinigameManager>();
        StartCoroutine(WaitForSpritesDownload());
    }

    private IEnumerator WaitForSpritesDownload()
    {
        while (!(m_toolSpritesInitialized && m_goodItemsSpritesInitialized && m_badItemsSpritesInitialized))
        {
            yield return null;
        }
        m_arMinigameManager.Inflate(m_numberOfItems, m_spawnRate, m_chanceOfBadItem, m_numberOfLives, m_lifetime, m_radius, m_toolSprites, m_goodItemsSprites, m_badItemsSprites, m_toolSoundEffect, m_goodItemSoundEffect, m_badItemSoundEffect);
        m_arMinigameManager.PlayerHitObject.RemoveAllListeners();
        m_arMinigameManager.PlayerHitObject.AddListener(OnPlayerHitObject);
    }

    protected override void OnInstructionPopinClosed()
    {
        if (!m_started)
        {
            m_arMinigameManager.StartGame(m_goodItemsSprites, m_badItemsSprites);
        }

        base.OnInstructionPopinClosed();
    }

    private void OnPlayerHitObject(ARGameItem aRGameItem)
    {
        if (aRGameItem.IsPositive)
        {
            m_numberOfHitItems++;
            m_arMinigameManager.GoodObjectHit(m_numberOfItems - m_numberOfHitItems);

            if (m_numberOfHitItems >= m_numberOfItems)
            {
                OnActivityOver(true);
                m_arMinigameManager.EndGame();
            }
        }
        else
        {
            m_numberOfLives--;
            m_arMinigameManager.BadObjectHit(m_numberOfLives);
            if (m_numberOfLives == 0)
            {
                OnActivityOver(false);
                m_arMinigameManager.EndGame();
            }
        }
    }
    #endregion Private

    #region Internals
    #endregion Internals
    #endregion Methods
}