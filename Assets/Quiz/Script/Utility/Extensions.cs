using UnityEngine;

namespace KanQuiz.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// Creates and returns a clone of any given scriptable object.
        /// </summary>
        public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
        {
            if (scriptableObject == null)
            {
                Debug.LogError($"ScriptableObject was null. Returning default {typeof(T)} object.");
                return (T)ScriptableObject.CreateInstance(typeof(T));
            }

            T instance = Object.Instantiate(scriptableObject);
            instance.name = scriptableObject.name; // remove (Clone) from name
            return instance;
        }

        public static bool ToBoolean(this string value)
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            return false;
        }

        public static int Modulo(this int value, int mod)
        {
            return (value % mod + mod) % mod;
        }

        
    }
}
