using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Data/BuildingDefinition")]
    public class BuildingDefinition : ScriptableObject, IIdentifiable
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private int _id;
        [SerializeField]
        private string _name;
        [SerializeField]
        private string _description;
        [SerializeField]
        private Sprite _icon;
        [SerializeField]
        private float _endurance;

        [SerializeField]
        private DateTimeWrapper _startTime = new DateTimeWrapper() { Value = new DateTime(2025, 11, 3, 17, 53, 0) };
        [SerializeField]
        private DateTimeWrapper _endTime;

        [SerializeField]
        private TimeSpanWrapper _totalDuration;

        [JsonIgnore]
        public int Id { get => _id; set => _id = value; }
        public string Name => _name;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float Endurance => _endurance;

        public DateTimeWrapper StartTime { get => _startTime; set => _startTime = value; }
        public DateTimeWrapper EndTime { get => _endTime; set => _endTime = value; }
        public TimeSpanWrapper TotalDuration { get => _totalDuration; set => _totalDuration = value; }

        [ContextMenu("Стартовая дата - текущая")]
        public void SetStartTimeToNow()
        {
            _startTime.Value = DateTime.Now;
        }
    }
}
