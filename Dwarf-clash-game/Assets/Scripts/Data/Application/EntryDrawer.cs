#if UNITY_EDITOR
using Data;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// CustomEditor для ResourceTemplateSO (UIToolkit, с ListView)
[CustomEditor(typeof(TemplateObject))]
public class ResourceTemplateSOEditor : Editor
{
    private const string _resourcesFieldName = "_resources";  // Имя приватного поля в ResourceTemplateSO
    private const string _resourcesFieldTitle = "Resource";

    // Вспомогательный класс для хранения данных в userData
    private class ButtonData
    {
        public int Index;
        public SerializedProperty ExternalIdProp;
        public IIdentifiable Resource;
        public int ExternalId;
    }

    private ListView _listView;
    private SerializedProperty _resourcesProp;

    private void OnEnable()
    {
        // Инициализируем resourcesProp один раз здесь (стандартная практика для CustomEditor)
        _resourcesProp = serializedObject.FindProperty(_resourcesFieldName);
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        // Resources список через ListView
        // resourcesProp уже инициализирован в OnEnable(), так что здесь не нужно повторять
        _listView = new ListView();
        _listView.itemsSource = ((TemplateObject)target).Resources; // Привязка к списку (используем публичное свойство)
        _listView.makeItem = () => CreateEntryElement(); // Создание элемента
        _listView.bindItem = (element, index) => BindEntryElement(element, index); // Привязка данных
        _listView.reorderable = true; // Разрешаем перетаскивание
        _listView.showAddRemoveFooter = false; // Убираем стандартный footer, добавим свой
        root.Add(_listView);

        // Кнопка Add Resource
        var addButton = new Button(() => AddNewResource());
        addButton.text = "Add Resource";
        root.Add(addButton);

        return root;
    }

    private VisualElement CreateEntryElement()
    {
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;

        // Поле Resource
        var resourceField = new ObjectField(_resourcesFieldTitle);
        resourceField.objectType = typeof(Object); // Или конкретный тип
        resourceField.style.flexGrow = 0;
        resourceField.name = _resourcesFieldTitle; // Для поиска
        container.Add(resourceField);

        // Поле ExternalId (readonly, теперь IntegerField)
        var externalIdField = new IntegerField("External ID");
        externalIdField.isReadOnly = true;
        externalIdField.style.flexGrow = 1;
        externalIdField.name = "External ID"; // Для поиска
        container.Add(externalIdField);

        // Кнопка Generate ID
        var generateButton = new Button();
        generateButton.text = "Generate ID";
        generateButton.name = "Generate ID"; // Для поиска
        container.Add(generateButton);

        // Кнопка Remove
        var removeButton = new Button();
        removeButton.text = "Remove";
        removeButton.name = "Remove"; // Для поиска
        container.Add(removeButton);

        // Кнопка Sync (всегда присутствует, но скрыта по умолчанию)
        var syncButton = new Button();
        syncButton.text = "Sync Resource ID";
        syncButton.name = "Sync"; // Для поиска
        syncButton.style.display = DisplayStyle.None; // Скрываем по умолчанию
        container.Add(syncButton);

        // Плейсхолдеры для предупреждений
        var warningLabel = new Label();
        warningLabel.style.color = Color.yellow;
        warningLabel.name = "Warning"; // Для поиска
        container.Add(warningLabel);

        var errorLabel = new Label();
        errorLabel.style.color = Color.red;
        errorLabel.name = "Error"; // Для поиска
        container.Add(errorLabel);

        return container;
    }

    private void BindEntryElement(VisualElement element, int index)
    {
        var entryProp = _resourcesProp.GetArrayElementAtIndex(index);
        var resourceProp = entryProp.FindPropertyRelative(_resourcesFieldTitle);
        var externalIdProp = entryProp.FindPropertyRelative("ExternalId");

        var resourceField = element.Q<ObjectField>(_resourcesFieldTitle);
        // УБИРАЕМ BindProperty, чтобы вручную управлять значением и ловить изменения (включая drag-and-drop)
        resourceField.value = resourceProp.objectReferenceValue;

        var externalIdField = element.Q<IntegerField>("External ID");
        externalIdField.BindProperty(externalIdProp);

        var generateButton = element.Q<Button>("Generate ID");
        var removeButton = element.Q<Button>("Remove");
        var syncButton = element.Q<Button>("Sync");
        var warningLabel = element.Q<Label>("Warning");
        var errorLabel = element.Q<Label>("Error");

        // Сохраняем данные в userData контейнера
        var data = new ButtonData
        {
            Index = index,
            ExternalIdProp = externalIdProp,
            Resource = resourceProp.objectReferenceValue as IIdentifiable,
            ExternalId = externalIdProp.intValue
        };
        element.userData = data;

        // Отписываем старые обработчики и подписываем новые
        resourceField.UnregisterCallback<ChangeEvent<Object>>(OnResourceChanged);
        resourceField.RegisterCallback<ChangeEvent<Object>>(OnResourceChanged);

        generateButton.UnregisterCallback<ClickEvent>(OnGenerateClicked);
        generateButton.RegisterCallback<ClickEvent>(OnGenerateClicked);

        removeButton.UnregisterCallback<ClickEvent>(OnRemoveClicked);
        removeButton.RegisterCallback<ClickEvent>(OnRemoveClicked);

        syncButton.UnregisterCallback<ClickEvent>(OnSyncClicked);
        syncButton.RegisterCallback<ClickEvent>(OnSyncClicked);

        // Скрываем кнопку Generate, если ExternalId уже существует ( != 0)
        generateButton.style.display = data.ExternalId != 0 ? DisplayStyle.None : DisplayStyle.Flex;

        // Проверка и синхронизация
        var resource = data.Resource;
        if (resource != null && data.ExternalId != 0 && resource.Id != data.ExternalId)
        {
            warningLabel.text = "Resource ID does not match External ID.";
            syncButton.style.display = DisplayStyle.Flex; // Показываем
        }
        else
        {
            warningLabel.text = "";
            syncButton.style.display = DisplayStyle.None; // Скрываем
        }

        if (resourceProp.objectReferenceValue != null && !(resourceProp.objectReferenceValue is IIdentifiable))
        {
            errorLabel.text = "Resource does not implement IIdentifiable.";
        }
        else
        {
            errorLabel.text = "";
        }
    }

    // Обработчик для изменения Resource (проверка на дубликаты)
    private void OnResourceChanged(ChangeEvent<Object> evt)
    {
        var resourceField = evt.target as ObjectField;
        var element = resourceField.parent as VisualElement; // Находим контейнер
        var data = element.userData as ButtonData;
        if (data == null) return;

        var targetSO = (TemplateObject)target;
        var newResource = evt.newValue as IIdentifiable;
        var errorLabel = element.Q<Label>("Error");

        // Вручную обновляем SerializedProperty
        var resourceProp = _resourcesProp.GetArrayElementAtIndex(data.Index).FindPropertyRelative(_resourcesFieldTitle);
        resourceProp.objectReferenceValue = evt.newValue;
        serializedObject.ApplyModifiedProperties();

        if (IsDuplicateResource(evt.newValue, data.Index))
        {
            errorLabel.text = "This resource is already in the list.";
            // Сбрасываем поле
            resourceProp.objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            resourceField.value = null; // Синхронизируем UI
            _listView.RefreshItems();
        }
        else
        {
            errorLabel.text = "";
            // Обновляем UI, если нужно
            _listView.RefreshItems();
        }
    }

    // Метод для проверки дубликатов
    private bool IsDuplicateResource(Object resource, int currentIndex)
    {
        if (resource == null) return false;
        for (int i = 0; i < _resourcesProp.arraySize; i++)
        {
            if (i == currentIndex) continue;
            var otherResource = _resourcesProp.GetArrayElementAtIndex(i).FindPropertyRelative(_resourcesFieldTitle).objectReferenceValue;
            if (otherResource == resource) return true;
        }
        return false;
    }

    // Обработчик для Generate ID
    private void OnGenerateClicked(ClickEvent evt)
    {
        var button = evt.target as Button;
        var element = button.parent as VisualElement;
        var data = element.userData as ButtonData;
        if (data == null) return;

        GenerateUniqueExternalId(data.ExternalIdProp, data.Index);
    }

    // Обработчик для Remove
    private void OnRemoveClicked(ClickEvent evt)
    {
        var button = evt.target as Button;
        var element = button.parent as VisualElement;
        var data = element.userData as ButtonData;
        if (data == null) return;

        ConfirmRemove(data.Index);
    }

    // Обработчик для Sync
    private void OnSyncClicked(ClickEvent evt)
    {
        var button = evt.target as Button;
        var element = button.parent as VisualElement;
        var data = element.userData as ButtonData;
        if (data == null) return;

        SyncResourceId(data.Resource, data.ExternalId);
        _listView.RefreshItems();
    }

    private void GenerateUniqueExternalId(SerializedProperty externalIdProp, int index)
    {
        var so = (TemplateObject)target;
        int newId;
        int attempts = 0;
        do
        {
            newId = Random.Range(1, int.MaxValue);
            attempts++;
            if (attempts > 100)
            {
                Debug.LogError("Failed to generate unique ID after 100 attempts.");
                return;
            }
        } while (so.Resources.Any(r => r.ExternalId == newId));

        externalIdProp.intValue = newId;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(so);
        _listView.RefreshItems(); // Обновляем ListView
    }

    private void ConfirmRemove(int index)
    {
        if (EditorUtility.DisplayDialog("Confirm Removal", "Are you sure you want to remove this resource?", "Yes", "No"))
        {
            _resourcesProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            _listView.RefreshItems();
        }
    }

    private void SyncResourceId(IIdentifiable resource, int externalId)
    {
        resource.Id = externalId;
        EditorUtility.SetDirty(resource as Object);
        AssetDatabase.SaveAssets();
    }

    private void AddNewResource()
    {
        _resourcesProp.InsertArrayElementAtIndex(_resourcesProp.arraySize);
        var newEntry = _resourcesProp.GetArrayElementAtIndex(_resourcesProp.arraySize - 1);
        newEntry.FindPropertyRelative(_resourcesFieldTitle).objectReferenceValue = null;
        newEntry.FindPropertyRelative("ExternalId").intValue = 0; // Пустое значение
        serializedObject.ApplyModifiedProperties();
        _listView.RefreshItems();
    }
}
#endif
