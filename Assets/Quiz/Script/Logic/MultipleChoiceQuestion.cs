using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KanQuiz
{
    [System.Serializable]
    public class MultipleChoiceQuestion : BaseQuestion
    {
        public int Correct;
        public StringsAnswer Answers = new StringsAnswer(new List<string>());

        public override List<string> GetAnswers()
        {
            return Answers.Answers;
        }

        public override bool IsAnswerCorrect(IAnswer answers)
        {
            List<string> correctAnswers = new();
            for (int i = 0; i < Correct; i++)
            {
                correctAnswers.Add(Answers.Answers[i]);
            }

            return correctAnswers.Contains((answers as StringsAnswer).Answers[0]);

            //All answer at once
            //false if the answer amount if more or less than the correct answer
            // if (Correct != (answers as StringsAnswer).Answers.Count) return false;
            // return !(answers as StringsAnswer).Answers.Except(correctAnswers).Any();
        }
    }
}
