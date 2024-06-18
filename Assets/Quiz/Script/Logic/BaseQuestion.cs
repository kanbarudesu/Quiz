using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KanQuiz
{
    public interface IAnswer { }

    [System.Serializable]
    public abstract class BaseQuestion
    {
        public List<Category> Categories = new();
        [TextArea(3, 5)]
        public string Question;
        public Sprite Sprite;

        public virtual List<string> GetAnswers() { return default; }
        public abstract bool IsAnswerCorrect(IAnswer answers);
    }

    [System.Serializable]
    public class StringsAnswer : IAnswer
    {
        public List<string> Answers;

        public StringsAnswer(List<string> answers)
        {
            Answers = answers;
        }
    }

    [System.Serializable]
    public class BooleanAnswer : IAnswer
    {
        public bool Answer;

        public BooleanAnswer(bool answer)
        {
            Answer = answer;
        }
    }
}
