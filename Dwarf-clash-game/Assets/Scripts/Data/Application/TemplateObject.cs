using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "TemplateObject", menuName = "Data/Application/TemplateObject")]
    public class TemplateObject : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public Object Resource;  // Ссылка на SO или префаб (Object для гибкости)
            [ReadOnly] public int ExternalId;  // Внешний ID, генерируется и нельзя менять руками
        }

        [SerializeField]
        private List<Entry> _resources = new List<Entry>();
        public List<Entry> Resources { get => _resources; set => _resources = value; }
    }
}