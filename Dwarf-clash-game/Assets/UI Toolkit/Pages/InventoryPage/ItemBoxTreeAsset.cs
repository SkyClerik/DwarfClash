using UnityEngine;
using UnityEngine.UIElements;

public class ItemBoxTreeAsset : VisualElement
{
    public ItemBoxTreeAsset()
    {
        VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("ItemBoxTemplate");
        if (visualTree != null)
        {
            visualTree.CloneTree(this);
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to load ItemBoxTemplate.uxml from Resources");
        }
    }
}