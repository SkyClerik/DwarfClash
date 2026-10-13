using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RootScriptEditor
{
    [CustomEditor(typeof(UI.MainSceneUIManager))]
    public class MainSceneUIManagerEditor : Editor
    {
        private UI.MainSceneUIManager _target;

        private void OnEnable()
        {
            _target = (UI.MainSceneUIManager)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            foreach (var page in _target.Pages)
            {
                var l = new Label($"{page.Key}");
                root.Add( l );
            }

            return root;
        }
    }
}
