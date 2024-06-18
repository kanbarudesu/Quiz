using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KanQuiz
{
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "KanQuiz/GameConfiguration", order = 0)]
    public class GameConfiguration : ScriptableObject
    {
        public List<Category> Categories = new();
        public Sprite AnyCategorySprite;

        public int QuestionAmount = 10;
        public int TimeLimit = 10;
        public int QuestionScore = 10;
    }
}
