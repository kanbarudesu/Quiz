using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace KanQuiz.Editor
{
    [CustomPropertyDrawer(typeof(StringsAnswer))]
    public class StringsAnswerDrawer : PropertyDrawer
    {
        SerializedProperty property;
        VisualElement answerDataContainer;
        ListView answerListView;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            this.property = property;
            var answersContainer = new VisualElement();
            answersContainer.style.flexDirection = FlexDirection.Row;

            answerDataContainer = new VisualElement();
            answerDataContainer.style.flexGrow = 1;
            answerDataContainer.AddToClassList("contentBackground");

            answerListView = InitializeListView();
            answerListView.BindProperty(property.FindPropertyRelative("Answers"));

            answersContainer.Add(answerListView);
            answersContainer.Add(answerDataContainer);

            return answersContainer;
        }

        private ListView InitializeListView()
        {
            var listView = new ListView();
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            listView.showAddRemoveFooter = true;
            listView.showBoundCollectionSize = false;
            listView.showBorder = true;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.style.minWidth = 175;
            listView.style.maxWidth = 175;
            listView.style.minHeight = 130;
            listView.style.maxHeight = 130;

            listView.makeItem = OnMakeItem;
            listView.bindItem += OnBindItem;
            listView.selectionChanged += OnSelectionChanged;
            listView.itemIndexChanged += OnItemIndexChanged;
            listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(OnAddListItem);
            return listView;
        }

        private Label OnMakeItem()
        {
            var label = new Label();
            label.RemoveFromClassList("unity-label");
            label.style.height = 12;
            label.style.overflow = Overflow.Hidden;
            return label;
        }

        private void OnItemIndexChanged(int index1, int index2)
        {
            answerListView.SetSelection(index2);
        }

        private void OnAddListItem()
        {
            var answersProperty = property.FindPropertyRelative("Answers");
            answersProperty.arraySize++;
            answersProperty.GetArrayElementAtIndex(answersProperty.arraySize - 1).stringValue = "New Answer";
            property.serializedObject.ApplyModifiedProperties();
        }

        private void OnSelectionChanged(IEnumerable<object> enumerable)
        {
            answerDataContainer.Clear();
            foreach (var selectedObject in enumerable)
            {
                var textField = new TextField("Answer");
                textField.BindProperty(selectedObject as SerializedProperty);
                textField.style.flexDirection = FlexDirection.Column;
                textField.style.alignItems = Align.Center;
                textField.style.unityTextAlign = TextAnchor.MiddleCenter;

                //Trying to look for the text element using the Name from debugger, but it doesn't work. using the class name works fine.
                var textElement = textField.Q("unity-text-input").Q(null, "unity-text-element");
                textElement.style.flexWrap = Wrap.Wrap;
                textElement.style.whiteSpace = WhiteSpace.Normal;
                textElement.style.width = 1;

                answerDataContainer.Add(textField);
            }
        }

        private void OnBindItem(VisualElement element, int index)
        {
            (element as Label).BindProperty(property.FindPropertyRelative("Answers").GetArrayElementAtIndex(index));
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
