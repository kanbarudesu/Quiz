using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;

namespace KanQuiz.Editor
{
    [CustomPropertyDrawer(typeof(QuestionTypes))]
    public class QuestionTypesDrawer : PropertyDrawer
    {
        SerializedProperty collectionProperty;
        List<string> typeClasses;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            collectionProperty = property.FindPropertyRelative("Collection");
            typeClasses = GetDerivedClass<BaseQuestion>().Select(x => x.GetType().FullName).ToList();
            typeClasses.Add("Any");

            var listView = InitializeListView();
            listView.BindProperty(property.FindPropertyRelative("Collection"));

            return listView;
        }

        private ListView InitializeListView()
        {
            var listView = new ListView
            {
                headerTitle = "Question Types",
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                showAddRemoveFooter = true,
                showBoundCollectionSize = false,
                showBorder = true,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showFoldoutHeader = true,
            };

            listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(OnAddListItem);

            return listView;
        }

        private void OnAddListItem()
        {
            GenericMenu menu = new GenericMenu();
            bool alreadyAdded = false;
            foreach (var item in typeClasses)
            {
                for (int i = 0; i < collectionProperty.arraySize; i++)
                {
                    if (collectionProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Type").stringValue == item)
                    {
                        alreadyAdded = true;
                        break;
                    }
                    alreadyAdded = false;
                }
                if (!alreadyAdded)
                {
                    menu.AddItem(new GUIContent(item), false, AddItem, item);
                }
            }
            menu.ShowAsContext();
        }

        private void AddItem(object item)
        {
            collectionProperty.InsertArrayElementAtIndex(collectionProperty.arraySize);
            collectionProperty.GetArrayElementAtIndex(collectionProperty.arraySize - 1).FindPropertyRelative("Type").stringValue = (string)item;
            collectionProperty.serializedObject.ApplyModifiedProperties();
        }

        private IEnumerable<T> GetDerivedClass<T>()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(T)))
                .Select(type => (T)Activator.CreateInstance(type));
        }
    }

    [CustomPropertyDrawer(typeof(QuestionType))]
    public class QuestionTypeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement();

            PropertyField namePropertyField = new PropertyField(property.FindPropertyRelative("Name"));
            PropertyField typePropertyField = new PropertyField(property.FindPropertyRelative("Type"));

            container.Add(namePropertyField);
            container.Add(typePropertyField);

            typePropertyField.SetEnabled(false);

            return container;
        }
    }
}
