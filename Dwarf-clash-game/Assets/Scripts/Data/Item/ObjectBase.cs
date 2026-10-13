using UnityEngine;

namespace Data
{
    public class ObjectBase : ScriptableObject
    {
        [SerializeField]
        private string _friendlyName;
        [SerializeField]
        private string _description;
        [SerializeField]
        private Sprite _icon;
        [SerializeField]
        private int _minCount;
        [SerializeField]
        private int _maxCount;

        public string Description => _description;
        public string FriendlyName => _friendlyName;
        public Sprite Icon => _icon;
        public int MinAmount => _minCount;
        public int MaxAmount => _maxCount;
    }
}