using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace KanQuiz.Editor
{
    public static class QuestionDataImporter
    {
        public static void ImportFromCSV(string path, QuestionsData target, GameConfiguration gameConfig)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            Undo.RecordObject(target, "Import Questions");
            target.Questions.Clear();

            var lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                var cols = lines[i].Split(',');

                string type = cols[0];
                string questionText = cols[1];
                string categoryRaw = cols[2];
                string answersRaw = cols[3];
                string correctRaw = cols[4];

                BaseQuestion question = CreateQuestion(
                    type,
                    questionText,
                    categoryRaw,
                    answersRaw,
                    correctRaw,
                    gameConfig
                );

                if (question != null)
                    target.Questions.Add(question);
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

        private static BaseQuestion CreateQuestion(
            string type,
            string questionText,
            string categoryRaw,
            string answersRaw,
            string correctRaw,
            GameConfiguration gameConfig)
        {
            BaseQuestion q = type switch
            {
                "Single" => CreateSingleChoice(answersRaw, correctRaw),
                "Multiple" => CreateMultipleChoice(answersRaw, correctRaw),
                "TrueFalse" => CreateTrueFalse(correctRaw),
                _ => null
            };

            if (q == null) return null;

            q.Question = questionText;
            q.Categories = ParseCategories(categoryRaw, gameConfig);

            return q;
        }

        private static SingleChoiceQuestion CreateSingleChoice(string answersRaw, string correctRaw)
        {
            var answers = answersRaw.Split('|');
            var list = new List<string>(answers);

            // Ensure correct answer is first for single choice question
            list.Remove(correctRaw);
            list.Insert(0, correctRaw);

            return new SingleChoiceQuestion
            {
                Answers = new StringsAnswer(list)
            };
        }

        private static MultipleChoiceQuestion CreateMultipleChoice(string answersRaw, string correctRaw)
        {
            return new MultipleChoiceQuestion
            {
                Answers = new StringsAnswer(new List<string>(answersRaw.Split('|'))),
                Correct = int.Parse(correctRaw)
            };
        }

        private static TrueFalseQuestion CreateTrueFalse(string correctRaw)
        {
            var answer = new BooleanAnswer(bool.Parse(correctRaw));
            return new TrueFalseQuestion
            {
                Answers = answer
            };
        }

        private static List<Category> ParseCategories(string raw, GameConfiguration gameConfig)
        {
            var list = new List<Category>();
            var names = raw.Split('|');

            foreach (var name in names)
            {
                var category = new Category { Name = name };
                list.Add(category);
                if (!gameConfig.Categories.Contains(category))
                    gameConfig.Categories.Add(category);
            }

            return list;
        }
    }
}
