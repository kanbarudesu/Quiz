using UnityEngine;
using UnityEditor;
using KanQuiz;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(BooleanAnswer))]
public class BooleanAnswerDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var answerProp = property.FindPropertyRelative("Answer");
        var toggle = new Toggle("Answer");

        if (answerProp != null)
        {
            toggle.BindProperty(answerProp);
        }
        else
        {
            toggle.SetEnabled(false);
        }

        return toggle;
    }
}