//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(Test4))]
//public class GpuInstancingTestSpawner : MonoBehaviour
//{
//    [Header("Spawner Settings")]
//    [SerializeField] private int _numberOfObjects = 5000;
//    [SerializeField] private float _areaSize = 10f;

//    private Test4 _test3Controller;

//    void Awake()
//    {
//        _test3Controller = GetComponent<Test4>();
//        if (_test3Controller == null)
//        {
//            Debug.LogError("Test3 component not found on the same GameObject!");
//            return;
//        }

//        GenerateTargets();
//    }

//    void GenerateTargets()
//    {
//        var targets = new List<Transform>(_numberOfObjects);
//        var holder = new GameObject("Spawned Targets Holder");
//        holder.transform.SetParent(this.transform);

//        for (int i = 0; i < _numberOfObjects; i++)
//        {
//            var go = new GameObject($"Target_{i}");
//            go.transform.SetParent(holder.transform);

//            float x = Random.Range(-_areaSize / 2, _areaSize / 2);
//            float y = Random.Range(-_areaSize / 2, _areaSize / 2);
//            go.transform.position = new Vector3(x, y, 0);

//            targets.Add(go.transform);
//        }

//        // Pass the generated targets to the Test3 script
//        _test3Controller._targetTransforms = targets;

//        Debug.Log($"Generated {_numberOfObjects} targets and assigned them to Test3.");
//    }
//}
