using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Wezit
{
    public class QuizAnswer : MonoBehaviour
    {
        #region Fields
        #region SerializeFields
        [SerializeField] private TextMeshProUGUI _answerText;
        [SerializeField] private Button _button;
        [SerializeField] private Animator _answerAnimator;
        #endregion
        #region Private
        private QuizAnswerModel m_answer;
        #endregion
        #endregion

        #region Properties
        public UnityEvent<QuizAnswerModel> AnswerClicked = new UnityEvent<QuizAnswerModel>();
        #endregion

        #region Methods
        #region Monobehaviour
        #endregion

        #region Public
        public void Inflate(QuizAnswerModel answer)
        {
            m_answer = answer;
            _answerText.text = StringUtils.CleanFromWezit(m_answer.AnswerText);
            _button.onClick.AddListener(OnButtonClicked);
        }

        public void DisableButton()
        {
            _button.enabled = false;
        }
        #endregion
        #region Internal
        #endregion
        #region Private
        private void OnButtonClicked()
        {
            AnswerClicked?.Invoke(m_answer);
            //_answerAnimator?.SetTrigger(m_Answer.IsCorrect ? "Correct" : "Wrong");
        }
        #endregion
        #endregion
    }
}
