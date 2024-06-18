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
        Toggle toggle = new Toggle("Answer");
        toggle.BindProperty(property);
        return toggle;
    }
}