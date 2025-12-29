using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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
            answerDataContainer.style.flexDirection = FlexDirection.Column;
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
                var textField = new TextField();
                textField.BindProperty(selectedObject as SerializedProperty);
                textField.multiline = true;
                textField.style.whiteSpace = WhiteSpace.Normal;

                answerDataContainer.Add(new Label("Answer"));
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
