using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.Reflection;

namespace KanQuiz.Editor
{
    public class QuizGenerator : EditorWindow
    {
        [MenuItem("Quiz/QuizGenerator")]
        private static void ShowWindow()
        {
            var window = GetWindow<QuizGenerator>();
            window.titleContent = new GUIContent("QuizGenerator");
            window.minSize = new Vector2(735, 500);
            window.Show();
        }

        private VisualTreeAsset quizGeneratorView;
        private TabbedMenuController tabController;

        private VisualElement root;
        private VisualElement questionsContentRight;
        private ObjectField questionsDataField;
        private ObjectField gameConfigurationField;
        private UnityEngine.Object initialQuestionsDataObject;
        private UnityEngine.Object initialGameConfigObject;
        private ListView questionList;
        private SerializedObject selectedQuestionsData;
        private SerializedObject selectedGameConfigData;
        private SerializedProperty selectedQuestionProperty;

        private IEnumerable<BaseQuestion> derivedClass;
        private List<Category> categories = new();

        private void OnEnable()
        {
            string[] folderPath = { "Assets/Quiz/Resources/" };
            foreach (var result in AssetDatabase.FindAssets("t:ScriptableObject", folderPath))
            {
                var path = AssetDatabase.GUIDToAssetPath(result);
                initialQuestionsDataObject = AssetDatabase.LoadAssetAtPath(path, typeof(QuestionsData));
                if (initialGameConfigObject == null)
                    initialGameConfigObject = AssetDatabase.LoadAssetAtPath(path, typeof(GameConfiguration));
                // if (initialQuestionsDataObject != null) break;
            }

            //Load Quiz Generator Visual Asset
            string[] quizGeneratorViewFolderPath = { "Assets/Quiz/Script/Editor/UI Document/" };
            foreach (var result in AssetDatabase.FindAssets("t:VisualTreeAsset", quizGeneratorViewFolderPath))
            {
                var quizGeneratorViewPath = AssetDatabase.GUIDToAssetPath(result);
                quizGeneratorView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(quizGeneratorViewPath);
            }

            InitializeTreeView();
            InitializeCategoriesList();

            tabController.OnSelectedTabChanged += OnSelectedTabChanged;
        }

        private void OnDisable()
        {
            tabController.OnSelectedTabChanged -= OnSelectedTabChanged;
        }

        private void InitializeCategoriesList()
        {
            categories.Clear();
            foreach (var category in (initialGameConfigObject as GameConfiguration).Categories)
            {
                categories.Add(category);
            }
        }

        private void InitializeTreeView()
        {
            root = rootVisualElement;
            root.Add(quizGeneratorView.Instantiate());

            tabController = new(root);
            tabController.RegisterTabCallbacks();
        }

        public void CreateGUI()
        {
            DrawQuestionContentDisplay();
            DrawGameConfigurationDataDisplay();
        }

        private void DrawQuestionContentDisplay()
        {
            derivedClass = GetDerivedClass<BaseQuestion>();
            questionsContentRight = root.Q<VisualElement>("QuestionsContentRight");

            questionList = root.Q<ListView>("QuestionsListView");
            questionList.Q<Button>("unity-list-view__add-button").clickable = new Clickable(OnAddQuestionItems);
            questionList.makeItem = CreateLabel;
            questionList.bindItem = OnBindQuestionItem;
            questionList.selectionChanged += OnQuestionItemSelectionChanged;
            questionList.itemIndexChanged += OnQuestionItemIndexChanged;

            // Load Question Data
            questionsDataField = root.Q<ObjectField>("QuestionsDataField");
            questionsDataField.objectType = typeof(QuestionsData);
            questionsDataField.RegisterValueChangedCallback(OnQuestionsDataChanged);
            questionsDataField.value = initialQuestionsDataObject;
        }

        private void DrawGameConfigurationDataDisplay()
        {
            // Load Game Config Data
            gameConfigurationField = root.Q<ObjectField>("GameConfigField");
            gameConfigurationField.objectType = typeof(GameConfiguration);
            gameConfigurationField.RegisterValueChangedCallback(OnGameConfigurationDataChanged);
            gameConfigurationField.value = initialGameConfigObject;

            var categoryListView = root.Q<ListView>("categoryListView");
            var selectedCategoryDataDisplay = root.Q<VisualElement>("categoryDataDisplay");
            var categoryDataDisplay = root.Q<VisualElement>("categoryDataDisplay");
            var gameConfigDisplay = root.Q<VisualElement>("gameConfigDisplay");

            categoryListView.BindProperty(selectedGameConfigData.FindProperty("Categories"));
            categoryListView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(OnAddCategoryItems);
            categoryListView.makeItem = CreateLabel;
            categoryListView.bindItem = (element, index) =>
            {
                var selectedCategoryProperty = selectedGameConfigData.FindProperty("Categories").GetArrayElementAtIndex(index);
                (element as Label).BindProperty(selectedCategoryProperty.FindPropertyRelative("Name"));
                selectedCategoryProperty.serializedObject.ApplyModifiedProperties();
            };
            categoryListView.selectionChanged += (obj) =>
            {
                categoryDataDisplay.Clear();
                foreach (var item in obj)
                {
                    var property = (item as SerializedProperty).Copy();

                    var nameField = new TextField("Name");
                    nameField.BindProperty(property.FindPropertyRelative("Name"));

                    var container = SpritePreviewField(property.FindPropertyRelative("Sprite"), "Sprite");

                    categoryDataDisplay.Add(nameField);
                    categoryDataDisplay.Add(container);

                    property.serializedObject.ApplyModifiedProperties();
                }
            };

            var container = SpritePreviewField(selectedGameConfigData.FindProperty("AnyCategorySprite"), "\"Any\" Category Sprite");
            IntegerField questionAmount = new IntegerField("Question Amount");
            IntegerField timeLimit = new IntegerField("Time Limit");
            IntegerField questionScore = new IntegerField("Question Score");

            questionAmount.BindProperty(selectedGameConfigData.FindProperty("QuestionAmount"));
            timeLimit.BindProperty(selectedGameConfigData.FindProperty("TimeLimit"));
            questionScore.BindProperty(selectedGameConfigData.FindProperty("QuestionScore"));

            gameConfigDisplay.Add(container);
            gameConfigDisplay.Add(questionAmount);
            gameConfigDisplay.Add(timeLimit);
            gameConfigDisplay.Add(questionScore);
        }

        private IMGUIContainer SpritePreviewField(SerializedProperty property, string label)
        {
            Texture2D previewTexture = AssetPreview.GetAssetPreview(property.objectReferenceValue);
            var container = new IMGUIContainer(() =>
            {
                var field = EditorGUILayout.ObjectField(label, property.objectReferenceValue, typeof(Sprite), false);
                property.objectReferenceValue = field;
                property.serializedObject.ApplyModifiedProperties();
            });
            container.style.marginLeft = 3;
            container.style.marginRight = 3;
            container.style.marginTop = 1;
            container.style.marginBottom = 1;
            container.style.alignSelf = Align.FlexStart;

            return container;
        }

        private void OnQuestionItemIndexChanged(int index1, int index2)
        {
            questionList.SetSelection(index2);
        }

        private void OnQuestionItemSelectionChanged(IEnumerable<object> obj)
        {
            questionsContentRight.Clear();
            foreach (var item in obj)
            {
                var iterator = (item as SerializedProperty).Copy();
                selectedQuestionProperty = iterator.Copy();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    // Make sure to set enterChildren to false for subsequent iterations
                    enterChildren = false;

                    if (iterator.name == "data") continue;
                    if (iterator.name == "Categories")
                    {
                        ListView categoryList = InitializeCategoryListView();
                        categoryList.BindProperty(iterator);
                        categoryList.makeItem = CreateLabel;
                        categoryList.bindItem = OnBindCategoryItem;
                        categoryList.itemIndexChanged += (index1, index2) =>
                        {
                            categoryList.SetSelection(index2);
                        };
                        questionsContentRight.Add(categoryList);
                        continue;
                    }
                    if (iterator.name == "Sprite")
                    {
                        var container = SpritePreviewField(iterator.Copy(), "Sprite");
                        questionsContentRight.Add(container);
                        continue;
                    }

                    var propertyField = new PropertyField(iterator);
                    propertyField.BindProperty(iterator);
                    questionsContentRight.Add(propertyField);
                }
                (item as SerializedProperty).serializedObject.ApplyModifiedProperties();
            }
        }

        private ListView InitializeCategoryListView()
        {
            var categoryList = new ListView();
            categoryList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            categoryList.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            categoryList.showAddRemoveFooter = true;
            categoryList.headerTitle = "Category";
            categoryList.showBoundCollectionSize = false;
            categoryList.showFoldoutHeader = true;
            categoryList.showBorder = true;
            categoryList.reorderable = true;
            categoryList.reorderMode = ListViewReorderMode.Animated;

            var addButton = categoryList.Q<Button>("unity-list-view__add-button");
            addButton.clickable = new Clickable(OnAddCategoryToQuestionData);
            return categoryList;
        }

        private void OnAddCategoryToQuestionData()
        {
            GenericMenu addCategoryMenu = new GenericMenu();
            var selectedCategoriesProperty = selectedQuestionProperty.FindPropertyRelative("Categories");
            bool alreadyAdded = false;
            foreach (var category in categories)
            {
                for (int i = 0; i < selectedCategoriesProperty.arraySize; i++)
                {
                    if (selectedCategoriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue == category.Name)
                    {
                        alreadyAdded = true;
                        break;
                    }
                    alreadyAdded = false;
                }
                if (!alreadyAdded)
                    addCategoryMenu.AddItem(new GUIContent(category.Name), false, OnCategoryMenuItemSelected, category);
            }
            addCategoryMenu.ShowAsContext();
        }

        private void OnCategoryMenuItemSelected(object category)
        {
            var selectedCategoriesProperty = selectedQuestionProperty.FindPropertyRelative("Categories");
            selectedCategoriesProperty.InsertArrayElementAtIndex(selectedCategoriesProperty.arraySize);
            selectedCategoriesProperty.GetArrayElementAtIndex(selectedCategoriesProperty.arraySize - 1).FindPropertyRelative("Name").stringValue = (category as Category).Name;
            selectedCategoriesProperty.GetArrayElementAtIndex(selectedCategoriesProperty.arraySize - 1).FindPropertyRelative("Sprite").objectReferenceValue = (category as Category).Sprite as UnityEngine.Object;
            selectedCategoriesProperty.serializedObject.ApplyModifiedProperties();
        }

        private void OnBindCategoryItem(VisualElement element, int index)
        {
            (element as Label).BindProperty(selectedQuestionProperty.FindPropertyRelative("Categories").GetArrayElementAtIndex(index).FindPropertyRelative("Name"));
            selectedQuestionProperty.serializedObject.ApplyModifiedProperties();
        }

        private IEnumerable<T> GetDerivedClass<T>()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(T)))
                .Select(type => (T)Activator.CreateInstance(type));
        }

        private void OnAddQuestionItems()
        {
            if (questionsDataField.value == null) return;

            GenericMenu addNewQuestionMenu = new GenericMenu();
            foreach (var item in derivedClass)
            {
                addNewQuestionMenu.AddItem(new GUIContent(item.GetType().Name), false, OnQuestionMenuItemSelected, item);
            }
            addNewQuestionMenu.ShowAsContext();
        }

        private void OnAddCategoryItems()
        {
            if (gameConfigurationField.value == null) return;
            var data = gameConfigurationField.value as GameConfiguration;
            data.Categories.Add(new Category() { Name = "New Category" });
        }

        private void OnQuestionMenuItemSelected(object item)
        {
            var data = questionsDataField.value as QuestionsData;
            var newItem = item as BaseQuestion;
            newItem.Question = "New " + newItem.GetType().Name;
            data.Questions.Add(newItem);
        }

        private VisualElement CreateLabel()
        {
            var label = new Label();
            label.RemoveFromClassList("unity-label");
            label.style.height = 12;
            label.style.overflow = Overflow.Hidden;
            return label;
        }

        private void OnBindQuestionItem(VisualElement element, int index)
        {
            var property = new SerializedObject(questionsDataField.value);
            (element as Label).BindProperty(property.FindProperty("Questions").GetArrayElementAtIndex(index).FindPropertyRelative("Question"));
        }

        private void OnQuestionsDataChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            if (evt.newValue != null)
            {
                questionsContentRight.Clear();
                root.Q<VisualElement>("questionContentHolder").RemoveFromClassList("unselectedContent");
                selectedQuestionsData = new SerializedObject(evt.newValue);
                questionList.BindProperty(selectedQuestionsData.FindProperty("Questions"));
                return;
            }
            root.Q<VisualElement>("questionContentHolder").AddToClassList("unselectedContent");
        }

        private void OnGameConfigurationDataChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            if (evt.newValue != null)
            {
                root.Q<VisualElement>("gameConfigContentHolder").RemoveFromClassList("unselectedContent");
                selectedGameConfigData = new SerializedObject(evt.newValue);
                return;
            }
            root.Q<VisualElement>("gameConfigContentHolder").AddToClassList("unselectedContent");
        }

        private void OnSelectedTabChanged(VisualElement tab)
        {
            InitializeCategoriesList();
        }

        private string[] GetFields(System.Type type)
        {
            List<string> fields = new List<string>();
            BindingFlags bindingFlags = BindingFlags.DeclaredOnly | // This flag excludes inherited variables.
                                        BindingFlags.Public |
                                        BindingFlags.NonPublic |
                                        BindingFlags.Instance |
                                        BindingFlags.Static;
            foreach (FieldInfo field in type.GetFields(bindingFlags))
            {
                fields.Add(field.Name);
            }
            return fields.ToArray();
        }
    }
}