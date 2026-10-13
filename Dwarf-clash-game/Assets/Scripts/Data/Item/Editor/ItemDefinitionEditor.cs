using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Data
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefinitionEditor : Editor
    {
        private ItemDefinition _itemDefinition;

        private void OnEnable()
        {
            _itemDefinition = target as ItemDefinition;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            Sprite icon = _itemDefinition.Icon;
            if (icon != null)
            {
                VisualElement element = new VisualElement();
                float originalWidth = icon.rect.width;
                float originalHeight = icon.rect.height;

                float minWidth = 128f;
                float maxWidth = 256f;

                float width = Mathf.Clamp(originalWidth, minWidth, maxWidth);
                float height = (originalHeight / originalWidth) * width;

                element.style.backgroundImage = new StyleBackground(icon);
                element.style.width = width;
                element.style.height = height;

                root.Add(element);
            }

            return root;
        }

        private Button CreateButton(string text, System.Action callback)
        {
            Button element = new Button();
            element.text = text;
            element.clicked += callback;
            return element;
        }
    }
}