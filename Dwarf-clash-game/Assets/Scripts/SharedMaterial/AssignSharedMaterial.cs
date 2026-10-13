using UnityEngine;

public class AssignSharedMaterial : MonoBehaviour
{
    [SerializeField] private Material sharedMaterial; // ссылка на общий материал

    [SerializeField] private Renderer[] targetRenderers; // массив рендереров, которым назначим материал

    void Start()
    {
        if (sharedMaterial == null)
        {
            Debug.LogError("Shared Material не назначен!");
            return;
        }

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer != null)
            {
                // »спользуем sharedMaterial Ч один экземпл€р дл€ всех
                renderer.sharedMaterial = sharedMaterial;
            }
        }

        Debug.Log($"Ќазначен общий материал {sharedMaterial.name} на {targetRenderers.Length} объектов.");
    }
}