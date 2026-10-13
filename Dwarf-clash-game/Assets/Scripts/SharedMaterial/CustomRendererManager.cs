using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Toolbox;

public class CustomRendererManager : Singleton<CustomRendererManager>
{
    [Header("Настройки")]
    public Material material; // Материал с URP Unlit Shader (включите Enable Instancing)
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>(); // Теперь MeshRenderer
    public List<MeshRenderer> MeshRenderers => meshRenderers; // Публичный геттер
    public Camera mainCamera;
    public float movementThreshold = 0.01f;
    public float rotationThreshold = 0.1f;
    [Tooltip("Bake: Порог для статичных объектов (не обновлять чаще)")]
    public int bakeUpdateFrameInterval = 10; // Обновлять bake каждые N кадров

    // Кеш данных для каждого объекта
    private struct ObjectData
    {
        public Vector3 lastPosition;
        public Quaternion lastRotation;
        public Matrix4x4 matrix;
        public float distance;
        public bool isVisible;
        public Mesh lastMesh;
        public Material lastMaterial;
        public int lastLayer;
        public bool isBaked; // Флаг для bake: true, если объект статичный
    }
    private ObjectData[] cachedData;

    // Bake: Предварительные группы для статичных объектов (слой -> материал -> меш -> список матриц)
    private Dictionary<int, Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>> bakedGroups = new Dictionary<int, Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>>();
    private int frameCounter = 0;

    // Пулы для снижения GC (исправлены типы)
    private Stack<List<Matrix4x4>> matrixListPool = new Stack<List<Matrix4x4>>();
    private Stack<Dictionary<Mesh, List<Matrix4x4>>> meshDictPool = new Stack<Dictionary<Mesh, List<Matrix4x4>>>();
    private Stack<Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>> matDictPool = new Stack<Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>>();
    private Stack<Dictionary<int, Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>>> layerDictPool = new Stack<Dictionary<int, Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>>>();

    // Job для сортировки
    [BurstCompile]
    private struct SortJob : IJob
    {
        public NativeArray<float> distances;
        public NativeArray<Matrix4x4> matrices;
        public int count;

        public void Execute()
        {
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (distances[i] < distances[j])
                    {
                        float tempDist = distances[i];
                        distances[i] = distances[j];
                        distances[j] = tempDist;
                        Matrix4x4 tempMat = matrices[i];
                        matrices[i] = matrices[j];
                        matrices[j] = tempMat;
                    }
                }
            }
        }
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (material == null)
        {
            Debug.LogError("Укажите Material!");
            return;
        }
        InitializeCache();
    }

    private void InitializeCache()
    {
        if (meshRenderers == null) meshRenderers = new List<MeshRenderer>();
        cachedData = new ObjectData[meshRenderers.Count];
        for (int i = 0; i < cachedData.Length; i++)
        {
            cachedData[i] = new ObjectData { lastPosition = Vector3.zero, lastRotation = Quaternion.identity, isVisible = true, lastMesh = null, lastMaterial = null, lastLayer = -1, isBaked = false };
        }
    }

    // Метод для добавления MeshRenderer
    public void AddMeshRenderer(MeshRenderer mr)
    {
        if (mr == null || meshRenderers.Contains(mr)) return;
        meshRenderers.Add(mr);
        System.Array.Resize(ref cachedData, cachedData.Length + 1);
        cachedData[cachedData.Length - 1] = new ObjectData { lastPosition = Vector3.zero, lastRotation = Quaternion.identity, isVisible = true, lastMesh = null, lastMaterial = null, lastLayer = -1, isBaked = false };
        Debug.Log($"Добавлен объект: {mr.gameObject.name}");
        mr.enabled = false;
    }

    // Метод для удаления MeshRenderer
    public void RemoveMeshRenderer(MeshRenderer mr)
    {
        if (mr == null || !meshRenderers.Contains(mr)) return;
        int index = meshRenderers.IndexOf(mr);
        meshRenderers.RemoveAt(index);
        for (int i = index; i < cachedData.Length - 1; i++)
        {
            cachedData[i] = cachedData[i + 1];
        }
        System.Array.Resize(ref cachedData, cachedData.Length - 1);
        Debug.Log($"Удалён объект: {mr.gameObject.name}");
    }

    void Update()
    {
        if (meshRenderers.Count == 0) return;
        if (cachedData.Length != meshRenderers.Count) InitializeCache();

        Vector3 cameraPos = mainCamera.transform.position;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        frameCounter++;
        bool updateBake = frameCounter % bakeUpdateFrameInterval == 0;

        // Очищаем динамические группы (теперь правильный тип)
        var dynamicGroups = GetLayerDictFromPool(); // Слой -> Материал -> Меш -> Список матриц

        // Обновляем кеш и группируем
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            var mr = meshRenderers[i];
            if (mr == null || mr.sharedMaterial == null || mr.GetComponent<MeshFilter>() == null) continue;

            Vector3 pos = mr.transform.position;
            Quaternion rot = mr.transform.rotation;
            Mesh mesh = mr.GetComponent<MeshFilter>().sharedMesh;
            Material mat = mr.sharedMaterial;
            int layer = mr.gameObject.layer;

            bool needsUpdate = Vector3.Distance(pos, cachedData[i].lastPosition) > movementThreshold ||
                               Quaternion.Angle(rot, cachedData[i].lastRotation) > rotationThreshold ||
                               mesh != cachedData[i].lastMesh || mat != cachedData[i].lastMaterial || layer != cachedData[i].lastLayer;

            Bounds bounds = new Bounds(pos, Vector3.one * 1f);
            bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
            cachedData[i].isVisible = isVisible;

            if (needsUpdate)
            {
                cachedData[i].lastPosition = pos;
                cachedData[i].lastRotation = rot;
                cachedData[i].matrix = mr.transform.localToWorldMatrix;
                cachedData[i].distance = Vector3.Distance(pos, cameraPos);
                cachedData[i].lastMesh = mesh;
                cachedData[i].lastMaterial = mat;
                cachedData[i].lastLayer = layer;
                cachedData[i].isBaked = false; // Сбрасываем bake
            }

            // Bake: Если объект статичный и видимый, добавляем в bakedGroups
            if (!needsUpdate && isVisible && updateBake)
            {
                if (!bakedGroups.ContainsKey(layer))
                    bakedGroups[layer] = new Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>();
                if (!bakedGroups[layer].ContainsKey(mat))
                    bakedGroups[layer][mat] = new Dictionary<Mesh, List<Matrix4x4>>();
                if (!bakedGroups[layer][mat].ContainsKey(mesh))
                    bakedGroups[layer][mat][mesh] = new List<Matrix4x4>();
                bakedGroups[layer][mat][mesh].Add(cachedData[i].matrix);
                cachedData[i].isBaked = true;
            }
            else if (isVisible && cachedData[i].matrix != Matrix4x4.zero)
            {
                // Динамическая группа (теперь правильно: dynamicGroups[layer][mat][mesh])
                if (!dynamicGroups.ContainsKey(layer))
                    dynamicGroups[layer] = GetMatDictFromPool();
                if (!dynamicGroups[layer].ContainsKey(mat))
                    dynamicGroups[layer][mat] = GetMeshDictFromPool();
                if (!dynamicGroups[layer][mat].ContainsKey(mesh))
                    dynamicGroups[layer][mat][mesh] = GetListFromPool();
                dynamicGroups[layer][mat][mesh].Add(cachedData[i].matrix);
            }
        }

        // Рендерим bake (статичные)
        RenderGroups(bakedGroups, false);

        // Рендерим динамические
        RenderGroups(dynamicGroups, true);

        // Возвращаем в пул
        ReturnLayerDictToPool(dynamicGroups);
    }

    private void RenderGroups(Dictionary<int, Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>> groups, bool isDynamic)
    {
        foreach (var layerGroup in groups)
        {
            foreach (var matGroup in layerGroup.Value)
            {
                foreach (var meshGroup in matGroup.Value)
                {
                    var list = meshGroup.Value;
                    if (list.Count == 0) continue;

                    // Сортировка по расстоянию (только для динамичных)
                    if (isDynamic)
                    {
                        NativeArray<float> distances = new NativeArray<float>(list.Count, Allocator.TempJob);
                        NativeArray<Matrix4x4> matrices = new NativeArray<Matrix4x4>(list.Count, Allocator.TempJob);
                        for (int i = 0; i < list.Count; i++)
                        {
                            matrices[i] = list[i];
                            distances[i] = 0; // Упрощённо; в реальности сопоставьте с cachedData
                        }
                        SortJob sortJob = new SortJob { distances = distances, matrices = matrices, count = list.Count };
                        JobHandle handle = sortJob.Schedule();
                        handle.Complete();
                        list.Clear();
                        for (int i = 0; i < matrices.Length; i++) list.Add(matrices[i]);
                        distances.Dispose();
                        matrices.Dispose();
                    }

                    // Рендерим батчами
                    int batchSize = 1023;
                    for (int i = 0; i < list.Count; i += batchSize)
                    {
                        int count = Mathf.Min(batchSize, list.Count - i);
                        Matrix4x4[] batchMatrices = new Matrix4x4[count];
                        for (int j = 0; j < count; j++) batchMatrices[j] = list[i + j];
                        Graphics.DrawMeshInstanced(meshGroup.Key, 0, matGroup.Key, batchMatrices);
                    }
                }
            }
        }
    }

    // Реализации пулов (исправлены)
    private List<Matrix4x4> GetListFromPool()
    {
        return matrixListPool.Count > 0 ? matrixListPool.Pop() : new List<Matrix4x4>();
    }

    private void ReturnListToPool(List<Matrix4x4> list)
    {
        list.Clear();
        matrixListPool.Push(list);
    }

    private Dictionary<Mesh, List<Matrix4x4>> GetMeshDictFromPool()
    {
        return meshDictPool.Count > 0 ? meshDictPool.Pop() : new Dictionary<Mesh, List<Matrix4x4>>();
    }

    private void ReturnMeshDictToPool(Dictionary<Mesh, List<Matrix4x4>> dict)
    {
        dict.Clear();
        meshDictPool.Push(dict);
    }

    private Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>> GetMatDictFromPool()
    {
        return matDictPool.Count > 0 ? matDictPool.Pop() : new Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>();
    }

    private void ReturnMatDictToPool(Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>> dict)
    {
        foreach (var meshList in dict.Values)
        {
            foreach (var list in meshList.Values)
            {
                ReturnListToPool(list);
            }
            ReturnMeshDictToPool(meshList);
        }
        dict.Clear();
        matDictPool.Push(dict);
    }

    private Dictionary<int, Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>> GetLayerDictFromPool()
    {
        return layerDictPool.Count > 0 ? layerDictPool.Pop() : new Dictionary<int, Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>>();
    }

    private void ReturnLayerDictToPool(Dictionary<int, Dictionary<Material, Dictionary<Mesh, List<Matrix4x4>>>> dict)
    {
        foreach (var matDict in dict.Values)
        {
            ReturnMatDictToPool(matDict);
        }
        dict.Clear();
        layerDictPool.Push(dict);
    }

    void OnDestroy()
    {
        // Очистка пулов
        matrixListPool.Clear();
        meshDictPool.Clear();
        matDictPool.Clear();
        layerDictPool.Clear();
        bakedGroups.Clear();
    }
}
