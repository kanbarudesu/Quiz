using System.Collections;
using System.Collections.Generic;
using KanQuiz.Utility;

namespace KanQuiz
{
    [System.Serializable]
    public class TrueFalseQuestion : BaseQuestion
    {
        public BooleanAnswer Answers;

        public override bool IsAnswerCorrect(IAnswer answers)
        {
            return (answers as BooleanAnswer).Answer == Answers.Answer;
        }
    }
}
