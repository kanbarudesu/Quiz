using System;
using UnityEngine;

namespace KanQuiz
{
    [System.Serializable]
    public class Category : IEquatable<Category>
    {
        public string Name;
        public Sprite Sprite;

        public bool Equals(Category other)
        {
            if (other == null) return false;
            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Category);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
    }
}
