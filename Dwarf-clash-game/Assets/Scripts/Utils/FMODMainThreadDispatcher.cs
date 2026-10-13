using UnityEngine;
using System;
using System.Collections.Concurrent;

namespace Core.Utils
{
    public class FMODMainThreadDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
        private static FMODMainThreadDispatcher _instance;

        // Этот атрибут гарантирует, что диспетчер будет создан автоматически при старте игры
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            if (_instance == null)
            {
                var go = new GameObject("FMODMainThreadDispatcher");
                _instance = go.AddComponent<FMODMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
        }

        /// <summary>
        /// Помещает действие в очередь для выполнения в основном потоке.
        /// Этот метод потокобезопасен.
        /// </summary>
        public static void Enqueue(Action action)
        {
            _executionQueue.Enqueue(action);
        }

        // Update вызывается в основном потоке и выполняет действия из очереди.
        private void Update()
        {
            while (_executionQueue.TryDequeue(out Action action))
            {
                action.Invoke();
            }
        }

        // Гарантирует, что в сцене есть только один экземпляр диспетчера.
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}