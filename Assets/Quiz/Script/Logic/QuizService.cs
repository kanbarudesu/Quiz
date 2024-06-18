using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace KanQuiz
{
    public class QuizService
    {
        QuestionsData questionsData;
        Stack<BaseQuestion> questionPool = new Stack<BaseQuestion>();

        public QuizService(QuestionsData questionsData)
        {
            this.questionsData = questionsData;
        }

        public void ShuffleQuestion(Type type = null, Category category = null)
        {
            questionPool = new Builder(questionsData).WithCategory(category).WithType(type).Shuffle().Build();
        }

        public int GetQuestionCount()
        {
            return questionPool.Count;
        }

        public BaseQuestion GetQuestion()
        {
            return questionPool.Pop();
        }

        public bool TryGetQuestion(out BaseQuestion question)
        {
            return questionPool.TryPop(out question);
        }

        private class Builder
        {
            QuestionsData questionData;
            Category category = null;
            Type type = null;
            bool isShuffle = false;

            Stack<BaseQuestion> questions;

            public Builder(QuestionsData questionsData)
            {
                this.questionData = questionsData;
                questions = new Stack<BaseQuestion>(questionData.Questions);
            }

            public Builder WithCategory(Category category)
            {
                this.category = category;
                return this;
            }

            public Builder WithType(Type type)
            {
                this.type = type;
                return this;
            }

            public Builder Shuffle()
            {
                isShuffle = true;
                return this;
            }

            public Stack<BaseQuestion> Build()
            {
                if (category != null)
                {
                    questions = new Stack<BaseQuestion>(questions.Where(questions => questions.Categories
                                                                        .Any(c => c.Name == category.Name)));
                }
                if (type != null)
                {
                    questions = new Stack<BaseQuestion>(questions.Where(questions => questions.GetType() == type));
                }
                if (isShuffle)
                {
                    System.Random random = new System.Random();
                    questions = new Stack<BaseQuestion>(questions.OrderBy(x => random.Next()));
                }
                return questions;
            }

        }
    }

}