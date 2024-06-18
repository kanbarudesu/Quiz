using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KanQuiz
{
    public class GameDisplay : MonoBehaviour
    {
        [SerializeField] private UIDocument quizMenuDocument;

        VisualElement root;
        QuizManager quizManager;

        //content container
        VisualElement currentActiveContent;
        VisualElement mainMenuContentContainer;
        VisualElement gameModeContentContainer;
        VisualElement categoryContentContainer;
        VisualElement questionContentContainer;

        //game mode content
        VisualElement questionTypeButtonsContainer;
        ButtonImage gameModeBackButton;
        ButtonImage gameModeNextButton;
        ButtonImage timerYesButton;
        ButtonImage timerNoButton;

        //category content
        VisualElement categoryButtonsContainer;
        ButtonImage categoryBackButton;
        ButtonImage categoryPlayButton;

        //question content
        VisualElement answerButtonsContainer;
        VisualElement questionImage;
        ButtonImage questionQuitButton;
        Label correctQuestionAmountLabel;
        Label questionLabel;
        Label scoresLabel;
        Label questionAmountRemainingLabel;
        Label questionCategoryLabel;
        RadialProgress timerLabel;

        //Main menu content
        ButtonImage mainMenuPlayButton;

        QuitGamePopup quitGamePopup;
        GameFinishedPopup gameFinishedPopup;

        ButtonImage currentSelectedGameModeButton;
        ButtonImage currentSelectedCategoryButton;

        private void Awake()
        {
            InitializeReferences();
            InitializeMainMenuContent();

            quizManager = QuizManager.Instance;
            quitGamePopup = new QuitGamePopup(root);
            gameFinishedPopup = new GameFinishedPopup(root);
        }

        private void OnEnable()
        {
            InitializeGameButtons();

            quizManager.OnGameStart += InitializeGameQuestionContent;
            quizManager.OnGameModeSelection += InitializeGameModeSelectionContent;
            quizManager.OnGameCategorySelection += InitializeCategorySelectionContent;
            quizManager.OnGameEnd += OnGameEnd;
            quizManager.OnShowNewQuestion += ShowQuestion;
            quizManager.Timer.OnTimerStart += OnTimerStart;
            quizManager.Timer.OnTickEvent += OnTimerTick;


            timerNoButton.AddToClassList("selected-button");
            timerYesButton.clicked += () =>
            {
                quizManager.UseTimer = true;
                timerYesButton.AddToClassList("selected-button");
                timerNoButton.RemoveFromClassList("selected-button");
            };
            timerNoButton.clicked += () =>
            {
                quizManager.UseTimer = false;
                timerNoButton.AddToClassList("selected-button");
                timerYesButton.RemoveFromClassList("selected-button");
            };
        }

        private void Update()
        {
            if (quizManager.Timer.IsRunning)
            {
                timerLabel.progress = quizManager.Timer.Progress;
            }
        }

        private void OnTimerStart()
        {
            timerLabel.maximumValue = quizManager.TimeLimit;
        }

        private void OnTimerTick(int currentTime)
        {
            timerLabel.SetText(currentTime.ToString());
        }

        private void OnGameEnd()
        {
            gameFinishedPopup.Show(quizManager.CurrentQuestionScore, quizManager.GetBestHighScore());
            quizManager.ResetGame();
        }

        private void InitializeReferences()
        {
            root = quizMenuDocument.rootVisualElement;

            //content container
            mainMenuContentContainer = root.Q<VisualElement>("main-menu-content-container");
            gameModeContentContainer = root.Q<VisualElement>("game-mode-content-container");
            categoryContentContainer = root.Q<VisualElement>("category-content-container");
            questionContentContainer = root.Q<VisualElement>("question-content-container");

            //game mode content
            questionTypeButtonsContainer = root.Q<VisualElement>("question-type__buttons-container");
            gameModeBackButton = root.Q<ButtonImage>("game-mode__back-button");
            gameModeNextButton = root.Q<ButtonImage>("game-mode__next-button");
            timerYesButton = root.Q<ButtonImage>("game-timer-yes-button");
            timerNoButton = root.Q<ButtonImage>("game-timer-no-button");

            //category content
            categoryButtonsContainer = root.Q<VisualElement>("category-buttons-container");
            categoryBackButton = root.Q<ButtonImage>("category__back-button");
            categoryPlayButton = root.Q<ButtonImage>("category__play-button");

            //question content
            answerButtonsContainer = root.Q<VisualElement>("answer-buttons-container");
            questionImage = root.Q<VisualElement>("question-image");
            questionQuitButton = root.Q<ButtonImage>("question__quit-button");
            questionLabel = root.Q<Label>("question-label");
            correctQuestionAmountLabel = root.Q<Label>("correct-question-amount-label");
            scoresLabel = root.Q<Label>("scores-label");
            timerLabel = root.Q<RadialProgress>("question-timer-radial");
            questionAmountRemainingLabel = root.Q<Label>("question-remaining-label");
            questionCategoryLabel = root.Q<Label>("question-category-label");

            //Main menu content
            mainMenuPlayButton = root.Q<ButtonImage>("menu-play-button");
        }

        private void InitializeMainMenuContent()
        {
            ShowContentContainer(mainMenuContentContainer);
        }

        private void InitializeGameModeSelectionContent(QuestionTypes gameModeTypes)
        {
            ShowContentContainer(gameModeContentContainer);
            questionTypeButtonsContainer.Clear();

            foreach (var gameModeType in gameModeTypes.Collection)
            {
                ButtonImage gameModeButton = new ButtonImage { labelText = gameModeType.Name };
                gameModeButton.AddToClassList("button-style");
                questionTypeButtonsContainer.Add(gameModeButton);
                if (gameModeType == quizManager.SelectedGameModeType)
                {
                    gameModeButton.AddToClassList("selected-button");
                    currentSelectedGameModeButton = gameModeButton;
                }
                gameModeButton.clicked += () =>
                {
                    quizManager.SelectedGameModeType = gameModeType;
                    gameModeButton.AddToClassList("selected-button");
                    if (currentSelectedGameModeButton != null)
                    {
                        currentSelectedGameModeButton.RemoveFromClassList("selected-button");
                    }
                    currentSelectedGameModeButton = gameModeButton;
                };
            }
        }

        private void InitializeCategorySelectionContent(List<Category> categories)
        {
            ShowContentContainer(categoryContentContainer);
            categoryButtonsContainer.Clear();

            foreach (var category in categories)
            {
                ButtonImage categoryButton = new ButtonImage { labelText = category.Name };
                categoryButton.AddToClassList("button-style");
                categoryButton.showImage = category.Sprite != null;
                categoryButton.Q("button-image").style.backgroundImage = new StyleBackground(category.Sprite);
                categoryButtonsContainer.Add(categoryButton);
                if (category == quizManager.SelectedCategory)
                {
                    categoryButton.AddToClassList("selected-button");
                    currentSelectedCategoryButton = categoryButton;
                }
                categoryButton.clicked += () =>
                {
                    quizManager.SelectedCategory = category;
                    categoryButton.AddToClassList("selected-button");
                    if (currentSelectedCategoryButton != null)
                    {
                        currentSelectedCategoryButton.RemoveFromClassList("selected-button");
                    }
                    currentSelectedCategoryButton = categoryButton;
                };
            }
        }

        private void InitializeGameQuestionContent()
        {
            ShowContentContainer(questionContentContainer);

            questionCategoryLabel.text = $"Category : {quizManager.SelectedCategory.Name}";       
            scoresLabel.text = $"Score : {quizManager.CurrentQuestionScore}";     

            if (quizManager.UseTimer)
            {
                timerLabel.RemoveFromClassList("hide-content");
            }
            else
            {
                timerLabel.AddToClassList("hide-content");
            }
        }

        public void ShowQuestion(BaseQuestion question)
        {
            questionAmountRemainingLabel.text = $"Question Remaining : {quizManager.CurrentQuestionAmount}";
            correctQuestionAmountLabel.text = $"{quizManager.CorrectQuestionAnswered} / {quizManager.QuestionAnswered}";
            questionLabel.text = question.Question;

            if (question.Sprite != null)
            {
                questionImage.style.backgroundImage = new StyleBackground(question.Sprite);
                questionImage.style.display = DisplayStyle.Flex;
            }
            else
            {
                questionImage.style.display = DisplayStyle.None;
            }

            if (question is MultipleChoiceQuestion || question is SingleChoiceQuestion)
            {
                answerButtonsContainer.RemoveFromClassList("horizontal-content");
                answerButtonsContainer.AddToClassList("vertical-content");
            }
            else if (question is TrueFalseQuestion)
            {
                answerButtonsContainer.RemoveFromClassList("vertical-content");
                answerButtonsContainer.AddToClassList("horizontal-content");
            }
            answerButtonsContainer.Clear();
        }

        public void SetEnableAnswerButtonsContainer(bool isEnable)
        {
            answerButtonsContainer.SetEnabled(isEnable);
        }

        public void AddAnswerButton(string answer, bool isCorrect, Action onClicked)
        {
            ButtonImage answerButton = new ButtonImage { labelText = answer };
            answerButton.AddToClassList("button-style");
            answerButtonsContainer.Add(answerButton);
            answerButton.clicked += () =>
            {
                onClicked?.Invoke();
                if (isCorrect)
                {
                    scoresLabel.text = $"Score : {quizManager.CurrentQuestionScore}";
                    answerButton.AddToClassList("correct-answer");
                }
                else
                {
                    answerButton.AddToClassList("incorrect-answer");
                }
            };
        }

        private void InitializeGameButtons()
        {
            mainMenuPlayButton.clicked += quizManager.GameModeSelection;

            gameModeBackButton.clicked += InitializeMainMenuContent;
            gameModeNextButton.clicked += quizManager.GameCategorySelection;

            categoryBackButton.clicked += quizManager.GameModeSelection;
            categoryPlayButton.clicked += quizManager.InitializeQuiz;

            questionQuitButton.clicked += Pause;
            quitGamePopup.OnNoButtonClicked += Resume;
            quitGamePopup.OnYesButtonClicked += OnQuitGame;

            gameFinishedPopup.OnGameRetry += quizManager.InitializeQuiz;
            gameFinishedPopup.OnGameQuit += OnQuitGame;
        }

        private void OnDisable()
        {
            mainMenuPlayButton.clicked -= quizManager.GameModeSelection;

            gameModeBackButton.clicked -= InitializeMainMenuContent;
            gameModeNextButton.clicked -= quizManager.GameCategorySelection;

            categoryBackButton.clicked -= quizManager.GameModeSelection;
            categoryPlayButton.clicked -= quizManager.InitializeQuiz;

            questionQuitButton.clicked -= Pause;
            quitGamePopup.OnNoButtonClicked -= Resume;
            quitGamePopup.OnYesButtonClicked -= OnQuitGame;

            gameFinishedPopup.OnGameRetry -= quizManager.InitializeQuiz;
            gameFinishedPopup.OnGameQuit -= OnQuitGame;
        }

        private void Pause()
        {
            quizManager.Timer.Pause();
            quitGamePopup.Show();
        }

        private void Resume()
        {
            quizManager.Timer.Resume();
        }

        private void OnQuitGame()
        {
            InitializeMainMenuContent();
            quizManager.ResetGame();
        }

        private void ShowContentContainer(VisualElement element)
        {
            if (element == currentActiveContent) return;

            element.RemoveFromClassList("hide-content");
            element.AddToClassList("entry-transition");
            if (currentActiveContent != null)
            {
                currentActiveContent.AddToClassList("exit-transition");
                currentActiveContent.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            }
            else
            {
                currentActiveContent = element;
            }

            void OnTransitionEnd(TransitionEndEvent evt)
            {
                currentActiveContent.AddToClassList("hide-content");
                currentActiveContent.RemoveFromClassList("entry-transition");
                currentActiveContent.RemoveFromClassList("exit-transition");
                currentActiveContent.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
                currentActiveContent = element;
            }
        }
    }
}
