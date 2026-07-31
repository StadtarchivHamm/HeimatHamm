using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class ARMusicGameCollectedNote : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private RawImage _note;
    #endregion
    #region Private
    private Wezit.Poi m_poi;
    #endregion
    #endregion

    #region Properties
    public Wezit.Poi Poi {  get { return m_poi; }}
    public Texture CollectedNoteThumbnail { get => _note.texture; }
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public ARMusicGameCollectedNote Inflate(Wezit.Poi poi)
    {
        m_poi = poi;
        ImageUtils.LoadRefImage(_note, this, m_poi, fillParent: false);

        return this;
    }
    #endregion
    #region Private

    #endregion
    #endregion
}
