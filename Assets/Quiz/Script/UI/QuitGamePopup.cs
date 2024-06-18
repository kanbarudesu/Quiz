using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KanQuiz
{
    public class QuitGamePopup
    {
        Button yesButton;
        Button noButton;

        VisualElement gameQuitPopupContainer;

        public event Action OnYesButtonClicked;
        public event Action OnNoButtonClicked;

        public QuitGamePopup(VisualElement root)
        {
            yesButton = root.Q<Button>("game-quit-yes-button");
            noButton = root.Q<Button>("game-quit-no-button");

            gameQuitPopupContainer = root.Q<VisualElement>("game-quit-popup");

            noButton.clicked += () =>
            {
                OnNoButtonClicked?.Invoke();
                Hide();
            };
            yesButton.clicked += () =>
            {
                OnYesButtonClicked?.Invoke();
                Hide();
            };
        }

        public void Show()
        {
            gameQuitPopupContainer.RemoveFromClassList("hide-content");
            gameQuitPopupContainer.AddToClassList("entry-transition");
        }

        public void Hide()
        {
            gameQuitPopupContainer.AddToClassList("exit-transition");
            gameQuitPopupContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            void OnTransitionEnd(TransitionEndEvent evt)
            {
                gameQuitPopupContainer.AddToClassList("hide-content");
                gameQuitPopupContainer.RemoveFromClassList("entry-transition");
                gameQuitPopupContainer.RemoveFromClassList("exit-transition");
                gameQuitPopupContainer.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            }
        }
    }
}
