using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Utils;

public class QuizIntermediateScreen : MonoBehaviour
{
    #region Fields
    #region SerializeFields
    [SerializeField] private Button _okayButton;
    [SerializeField] private RawImage _intermediateImage;
    [SerializeField] private GameObject _textContainer;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _description;
    #endregion
    #region Private
    private bool m_closeOnOkay;
    #endregion
    #endregion

    #region Properties
    public UnityEvent OkayClicked = new UnityEvent();
    #endregion

    #region Methods
    #region Monobehaviours
    #endregion
    #region Public
    public void Inflate(Wezit.QuizAnswerModel answer, bool closeOnOkay = true)
    {
        gameObject.SetActive(true);

        if (_title != null)
        {
            _title.text = answer.IntermediateScreenTitle;
        }

        if (_description != null) 
        {
            _description.text = answer.IntermediateScreenDescription;
        }

        if (_textContainer != null)
        {
            StartCoroutine(LayoutGroupRebuilder.Rebuild(_textContainer));
        }

        if (_intermediateImage != null)
        {
            answer.LoadIntermediateImage(_intermediateImage, this);
        }

        m_closeOnOkay = closeOnOkay;
        if (_okayButton != null)
        {
            _okayButton.onClick.RemoveAllListeners();
            _okayButton.onClick.AddListener(OnOkayButtonClicked);
        }
        else
        {
            StartCoroutine(WaitAndContinue(3));
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    #endregion
    #region Private
    private void OnOkayButtonClicked()
    {
        OkayClicked?.Invoke();
        if (m_closeOnOkay)
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator WaitAndContinue(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnOkayButtonClicked();
    }
    #endregion
    #endregion
}
