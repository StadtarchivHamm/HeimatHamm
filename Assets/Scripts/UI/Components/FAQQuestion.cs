using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FAQQuestion : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _content;
    #endregion
    #region Private

    #endregion
    #endregion

    #region Properties
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public void Inflate(string title, string content)
    {
        _title.text = title;
        _content.text = content;
    }
    #endregion
    #region Private

    #endregion
    #endregion
}
