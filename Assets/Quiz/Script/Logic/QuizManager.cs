using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KanQuiz.Utility;
using System.Linq;
using System;

namespace KanQuiz
{
    public class QuizManager : Singleton<QuizManager>
    {
        [SerializeField] private GameConfiguration gameConfiguration;
        [SerializeField] private QuestionsData questionsData;
        [SerializeField] private GameDisplay gameDisplay;
        [SerializeField] private QuestionTypes gameModeTypes;

        private QuizService quizService;

        private int correctAnswerAmount;
        private int correctQuestionScore;

        public int CurrentQuestionScore { get; private set; }
        public int CurrentQuestionAmount { get; private set; }
        public int QuestionAnswered { get; private set; }
        public int CorrectQuestionAnswered { get; private set; }
        public float TimeLimit { get; private set; }
        public bool UseTimer { get; set; }
        public QuestionType SelectedGameModeType { get; set; }
        public Category SelectedCategory { get; set; }
        private List<Category> gameCategories;

        public event Action<QuestionTypes> OnGameModeSelection;
        public event Action<List<Category>> OnGameCategorySelection;
        public event Action OnGameStart;
        public event Action OnGameEnd;
        public event Action<BaseQuestion> OnShowNewQuestion;

        public CountdownTimer Timer { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            quizService = new QuizService(questionsData.Clone());

            InitializeQuizSettings();

            //Initialize Category list
            gameCategories = new(gameConfiguration.Categories)
            {
                new Category() { Name = "Any", Sprite = gameConfiguration.AnyCategorySprite }
            };
            SelectedCategory = gameCategories[0];
            SelectedGameModeType = gameModeTypes.Collection[0];

            Timer = new CountdownTimer(TimeLimit);
            Timer.OnTimerStop += OnTimerStop;
        }

        private void Update()
        {
            if (UseTimer)
            {
                Timer.Tick(Time.deltaTime);
            }
        }

        private void InitializeQuizSettings()
        {
            TimeLimit = gameConfiguration.TimeLimit;
            correctQuestionScore = gameConfiguration.QuestionScore;
        }

        public void ResetGame()
        {
            Timer.Stop();
            CurrentQuestionAmount = 0;
            correctAnswerAmount = 0;
            CurrentQuestionScore = 0;
            CorrectQuestionAnswered = 0;
            QuestionAnswered = 0;
        }

        public void InitializeQuiz()
        {
            OnGameStart?.Invoke();
            if (CurrentQuestionAmount == 0)
            {
                InitializeQuizSettings();
                Type type = SelectedGameModeType.Type == "Any" ? null : Type.GetType(SelectedGameModeType.Type);
                Category category = SelectedCategory == gameCategories.Last() ? null : SelectedCategory;
                quizService.ShuffleQuestion(type, category);
                CurrentQuestionAmount = quizService.GetQuestionCount() < gameConfiguration.QuestionAmount ? quizService.GetQuestionCount() : gameConfiguration.QuestionAmount;
            }
            ShowQuestion();
        }

        public void GameModeSelection()
        {
            OnGameModeSelection?.Invoke(gameModeTypes);
        }

        public void GameCategorySelection()
        {
            OnGameCategorySelection?.Invoke(gameCategories);
        }

        public void ShowQuestion()
        {
            if (CurrentQuestionAmount <= 0) return;

            if (quizService.TryGetQuestion(out BaseQuestion question) == false)
            {
                QuestionGameFinished();
                return;
            }

            if (Timer.IsRunning && UseTimer)
            {
                Timer.Reset();
            }
            else if (!Timer.IsRunning && UseTimer)
            {
                Timer.Start();
            }

            gameDisplay.SetEnableAnswerButtonsContainer(true);
            OnShowNewQuestion?.Invoke(question);
            CurrentQuestionAmount--;

            switch (question)
            {
                case MultipleChoiceQuestion:
                    MultipleChoiceQuestionAnswer((MultipleChoiceQuestion)question);
                    break;
                case SingleChoiceQuestion:
                    SingleChoiceQuestionAnswer((SingleChoiceQuestion)question);
                    break;
                case TrueFalseQuestion:
                    TrueFalseQuestionAnswer((TrueFalseQuestion)question);
                    break;
                default: break;
            }
        }

        private void MultipleChoiceQuestionAnswer(MultipleChoiceQuestion question)
        {
            correctAnswerAmount = question.Correct;
            System.Random rand = new System.Random();
            List<string> correctAnswers = question.Answers.Answers.OrderBy(x => rand.Next()).ToList();

            foreach (var answer in correctAnswers)
            {
                IAnswer stringAnswer = new StringsAnswer(new List<string>() { answer });
                bool isCorrect = question.IsAnswerCorrect(stringAnswer);
                gameDisplay.AddAnswerButton(answer, isCorrect, () =>
                {
                    OnAnswerButtonClicked(question, stringAnswer);
                });
            }
        }

        private void SingleChoiceQuestionAnswer(SingleChoiceQuestion question)
        {
            correctAnswerAmount = 1;
            System.Random rand = new System.Random();
            List<string> correctAnswers = question.Answers.Answers.OrderBy(x => rand.Next()).ToList();

            foreach (var answer in correctAnswers)
            {
                IAnswer stringAnswer = new StringsAnswer(new List<string>() { answer });
                bool isCorrect = question.IsAnswerCorrect(stringAnswer);
                gameDisplay.AddAnswerButton(answer, isCorrect, () =>
                {
                    OnAnswerButtonClicked(question, stringAnswer);
                });
            }
        }

        private void TrueFalseQuestionAnswer(TrueFalseQuestion question)
        {
            correctAnswerAmount = 1;

            bool trueAnswer = question.IsAnswerCorrect(new BooleanAnswer(true));
            bool falseAnswer = question.IsAnswerCorrect(new BooleanAnswer(false));

            gameDisplay.AddAnswerButton("True", trueAnswer, () => { OnAnswerButtonClicked(question, new BooleanAnswer(true)); });
            gameDisplay.AddAnswerButton("False", falseAnswer, () => { OnAnswerButtonClicked(question, new BooleanAnswer(false)); });
        }

        private void OnAnswerButtonClicked(BaseQuestion question, IAnswer answer)
        {
            bool isCorrect = question.IsAnswerCorrect(answer);
            if (isCorrect)
            {
                //Do something if answer correct
                correctAnswerAmount--;
            }
            else
            {
                //Do something if answer incorrect
                correctAnswerAmount = 0;
            }

            if (correctAnswerAmount <= 0)
            {
                //wait a few second before showing next question
                CurrentQuestionScore += isCorrect ? correctQuestionScore : 0;
                CorrectQuestionAnswered += isCorrect ? 1 : 0;
                QuestionAnswered++;
                Timer.Pause();

                if (CurrentQuestionAmount <= 0)
                {
                    //all questions are answered
                    Invoke(nameof(QuestionGameFinished), 1f);
                }
                else
                {
                    GetNewQuestion();
                }
            }
        }

        private void QuestionGameFinished()
        {
            gameDisplay.SetEnableAnswerButtonsContainer(false);
            SaveBestHighScore();
            OnGameEnd?.Invoke();
        }

        private void GetNewQuestion()
        {
            gameDisplay.SetEnableAnswerButtonsContainer(false);
            Invoke(nameof(ShowQuestion), 2f);
        }

        private void OnTimerStop()
        {
            QuestionAnswered++;
            GetNewQuestion();
        }

        #region save load High Scores 
        public int GetBestHighScore()
        {
            return PlayerPrefs.GetInt("BestHighScore", CurrentQuestionScore);
        }

        [ContextMenu("Reset Best High Score")]
        private void ResetBestHighScore()
        {
            PlayerPrefs.DeleteKey("BestHighScore");
        }

        private void SaveBestHighScore()
        {
            int bestHighScore = GetBestHighScore();
            if (CurrentQuestionScore >= bestHighScore)
            {
                PlayerPrefs.SetInt("BestHighScore", CurrentQuestionScore);
            }
        }
        #endregion
    }
}
