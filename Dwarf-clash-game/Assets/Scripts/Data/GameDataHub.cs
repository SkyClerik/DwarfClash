using Data;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New GameDataHub", menuName = "Data/GameDataHub")]
    public class GameDataHub : ScriptableObject
    {
        [SerializeField]
        private TemplateObject _heroes;
        [SerializeField]
        private TemplateObject _GameCurrncy;
        [SerializeField]
        private TemplateObject _items;

        public TemplateObject Heroes => _heroes;
        public TemplateObject GameCurrncy => _GameCurrncy;
        public TemplateObject Items => _items;

        private void Awake()
        {
            DontDestroyOnLoad(target: this);
        }
    }
}