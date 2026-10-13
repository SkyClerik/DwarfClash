using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RootScriptEditor
{
    [CustomEditor(typeof(Data.MinionDefinition))]
    public class MinionDefinitionEditor : Editor
    {
        private Data.MinionDefinition _target;

        private void OnEnable()
        {
            _target = (Data.MinionDefinition)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            //var newButton = CreateButton("Generate ID", () =>
            //{
            //    _target.GenerateID();
            //});
            //root.Add(newButton);

            Sprite icon = _target.Icon;
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
