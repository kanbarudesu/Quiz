using System.Collections.Generic;
using UnityEngine;

namespace KanQuiz
{
    [System.Serializable]
    public class SingleChoiceQuestion : BaseQuestion
    {
        [SerializeReference]
        public StringsAnswer Answers = new StringsAnswer(new List<string>());

        public override List<string> GetAnswers()
        {
            return Answers.Answers;
        }

        public override bool IsAnswerCorrect(IAnswer answer)
        {
            var answers = (answer as StringsAnswer).Answers;
            return answers[0] == Answers.Answers[0];
        }
    }
}
