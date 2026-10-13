using UnityEngine;

public class CheckSharedMaterial : MonoBehaviour
{
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Debug.Log($"{name} использует материал: {renderer.sharedMaterial.name} (Instance ID: {renderer.sharedMaterial.GetInstanceID()})");
        }
    }
}