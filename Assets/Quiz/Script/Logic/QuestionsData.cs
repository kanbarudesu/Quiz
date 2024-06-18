using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KanQuiz
{
    [CreateAssetMenu(fileName = "QuestionsData", menuName = "KanQuiz/QuestionsData", order = 0)]
    public class QuestionsData : ScriptableObject
    {
        [SerializeReference]
        public List<BaseQuestion> Questions = new();

        public int GetQuestionCount() => Questions.Count;
    }
}
