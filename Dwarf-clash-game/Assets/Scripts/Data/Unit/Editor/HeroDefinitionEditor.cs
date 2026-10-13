using Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RootScriptEditor
{
    [CustomEditor(typeof(Data.HeroDefinition))]
    public class HeroDefinitionEditor : Editor
    {
        private Data.HeroDefinition _target;

        private void OnEnable()
        {
            _target = (Data.HeroDefinition)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            HeroIcon(root);

            var formationTitle = new Label("Formation");
            root.Add(formationTitle);

            if (_target.MinionDefinition != null)
                MinionIcon(root, _target.MinionDefinition);

            return root;
        }

        private void HeroIcon(VisualElement root)
        {
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
        }

        private void MinionIcon(VisualElement root, MinionDefinition minion)
        {
            Sprite icon = minion.Icon;
            if (icon != null)
            {
                VisualElement element = new VisualElement();
                float originalWidth = icon.rect.width;
                float originalHeight = icon.rect.height;

                float minWidth = 64f;
                float maxWidth = 128f;

                float width = Mathf.Clamp(originalWidth, minWidth, maxWidth);
                float height = (originalHeight / originalWidth) * width;

                element.style.backgroundImage = new StyleBackground(icon);
                element.style.width = width;
                element.style.height = height;

                root.Add(element);
            }
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