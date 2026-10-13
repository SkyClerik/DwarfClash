using UnityEngine;
using System.Collections.Generic;
using Toolbox;

public class InstancingManager : Singleton<InstancingManager>
{
    public List<GameObject> objects = new List<GameObject>(); // Можно оставить для ручного добавления в Inspector, если нужно
    public Material sharedMaterial; // Общий материал с Enable Instancing
    public Mesh sharedMesh; // Общий меш (Unity's Cube)

    private Matrix4x4[] matrices;
    private const int batchSize = 1023; // Лимит Unity

    // Метод для динамического добавления объекта
    public void AddObject(GameObject obj)
    {
        if (obj == null || objects.Contains(obj)) return; // Проверка на null и дубликаты
        objects.Add(obj);
        // Отключаем рендерер, чтобы избежать двойного рендеринга
        var renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;
    }

    // Метод для удаления объекта (опционально, для полноты)
    public void RemoveObject(GameObject obj)
    {
        if (obj == null || !objects.Contains(obj)) return;
        objects.Remove(obj);
        // Включаем рендерер обратно, если объект всё ещё существует
        var renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.enabled = true;
    }

    void Update()
    {
        if (objects.Count == 0) return;

        // Собираем матрицы для первого батча
        matrices = new Matrix4x4[Mathf.Min(objects.Count, batchSize)];
        for (int i = 0; i < matrices.Length; i++)
        {
            matrices[i] = objects[i].transform.localToWorldMatrix;
        }

        // Рендерим первый батч
        Graphics.DrawMeshInstanced(sharedMesh, 0, sharedMaterial, matrices);

        // Если больше 1023, рендерим дополнительные батчи
        for (int start = batchSize; start < objects.Count; start += batchSize)
        {
            int count = Mathf.Min(batchSize, objects.Count - start);
            Matrix4x4[] batchMatrices = new Matrix4x4[count];
            for (int i = 0; i < count; i++)
            {
                batchMatrices[i] = objects[start + i].transform.localToWorldMatrix;
            }
            Graphics.DrawMeshInstanced(sharedMesh, 0, sharedMaterial, batchMatrices);
        }
    }

    // В Start отключаем рендереры только для объектов, уже добавленных в Inspector
    void Start()
    {
        foreach (var obj in objects)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
