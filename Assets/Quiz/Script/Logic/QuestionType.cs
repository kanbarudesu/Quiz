using System;
using System.Collections.Generic;

namespace KanQuiz
{
    [Serializable]
    public class QuestionTypes
    {
        public List<QuestionType> Collection = new List<QuestionType>();
    }

    [Serializable]
    public class QuestionType
    {
        public string Name;
        public string Type;
    }
}
