using SimpleJSON;
using System.Collections;
using System.Collections.Generic;

namespace Wezit
{
    public class QuizQuestionModel
    {
        public string Title;
        public string Image;
        public bool EnableChrono;
        public int ChronoValue;
        public List<QuizAnswerModel> Answers;

        public QuizQuestionModel(string title, string imageSource, bool enableChrono, int chronoValue, JSONNode answersNode)
        {
            Title = title;
            Image = imageSource;
            EnableChrono = enableChrono;
            ChronoValue = chronoValue;

            Answers = new List<QuizAnswerModel>();
            foreach (JSONNode answerNode in answersNode)
            {
                QuizAnswerModel answer = new QuizAnswerModel(StringUtils.CleanFromWezit(answerNode["response.text.content"]), 
                                                             StringUtils.CleanFromWezit(answerNode["response.image"]), 
                                                             answerNode["response.status"],
                                                             !string.IsNullOrEmpty(answerNode["response.validation.title.text.content"]),
                                                             StringUtils.CleanFromWezit(answerNode["response.validation.title.text.content"]),
                                                             StringUtils.CleanFromWezit(answerNode["response.validation.description.text.content"]),
                                                             StringUtils.CleanFromWezit(answerNode["response.validation.image"]));
                Answers.Add(answer);
            }
        }
    }
}
