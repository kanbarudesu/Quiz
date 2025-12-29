using UnityEngine;

namespace KanQuiz
{
    [System.Serializable]
    public class TrueFalseQuestion : BaseQuestion
    {
        [SerializeReference]
        public BooleanAnswer Answers;

        public override bool IsAnswerCorrect(IAnswer answers)
        {
            return (answers as BooleanAnswer).Answer == Answers.Answer;
        }
    }
}
