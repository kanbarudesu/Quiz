using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KanQuiz
{
    public class GameFinishedPopup
    {
        VisualElement gameFinishedPopupContainer;
        VisualElement icon;

        Label scoresLabel;
        Label bestScoreLabel;

        Button retryButton;
        Button quitButton;

        public event Action OnGameRetry;
        public event Action OnGameQuit;

        public GameFinishedPopup(VisualElement root)
        {
            gameFinishedPopupContainer = root.Q<VisualElement>("game-finished-popup");

            retryButton = root.Q<Button>("game-finished-retry-button");
            quitButton = root.Q<Button>("game-finished-quit-button");

            icon = root.Q<VisualElement>("game-finished-icon");
            scoresLabel = root.Q<Label>("game-finished-score-label");
            bestScoreLabel = root.Q<Label>("game-finished-best-score-label");

            retryButton.clicked += () =>
            {
                OnGameRetry?.Invoke();
                Hide();
            };

            quitButton.clicked += () =>
            {
                OnGameQuit?.Invoke();
                Hide();
            };
        }

        public void Show(int scores, int bestScore)
        {
            string scoreTextColor = scores >= bestScore ? $"<color=green>{scores}</color>" : $"<color=red>{scores}</color>";
            scoresLabel.text = "Score " + scoreTextColor;
            bestScoreLabel.text = $"Best Score <color=yellow>{bestScore}</color>";

            gameFinishedPopupContainer.RemoveFromClassList("hide-content");
            gameFinishedPopupContainer.AddToClassList("entry-transition");
        }

        public void Hide()
        {
            gameFinishedPopupContainer.AddToClassList("exit-transition");
            gameFinishedPopupContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

            void OnTransitionEnd(TransitionEndEvent evt)
            {
                gameFinishedPopupContainer.AddToClassList("hide-content");
                gameFinishedPopupContainer.RemoveFromClassList("entry-transition");
                gameFinishedPopupContainer.RemoveFromClassList("exit-transition");
                gameFinishedPopupContainer.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            }
        }
    }
}
