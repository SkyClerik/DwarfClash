#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MonoBehaviour monoBehaviour = (MonoBehaviour)target;

        MethodInfo[] methods = monoBehaviour.GetType().GetMethods(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        GUI.enabled = Application.isPlaying;
        foreach (MethodInfo method in methods)
        {
            ButtonAttribute buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();
            if (buttonAttribute == null) continue;

            string buttonText = string.IsNullOrEmpty(buttonAttribute.Text)
                ? method.Name
                : buttonAttribute.Text;

            if (GUILayout.Button(buttonText))
            {
                method.Invoke(monoBehaviour, null);
            }
        }
    }
}
#endif