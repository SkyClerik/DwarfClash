using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class AggressionWeight
    {
        [SerializeField]
        private HeroDefinition _heroDefinition;
        [SerializeField]
        [Range(1, 10)]
        private int _weight = 1;

        public int Weight => _weight;
        public HeroDefinition HeroDefinition => _heroDefinition;
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "HeroDefinition", menuName = "Data/HeroDefinition")]
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroDefinition : ScriptableObject, IIdentifiable
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private int _id;

        [SerializeField]
        private string _name;
        [SerializeField]
        [TextArea(2,10)]
        private string _description;
        [SerializeField]
        private Sprite _icon;

        [JsonProperty]
        [SerializeField]
        private Rare _rare;
        [JsonProperty]
        [SerializeField]
        private int _level = 1;

        [Space]

        [SerializeField]
        [ReadOnly]
        [Tooltip("Текущее здоровье")]
        private float _curHealth;
        [SerializeField]
        [Tooltip("Максимальное здоровье")]
        private float _maxHealth;
        [SerializeField]
        [Tooltip("Урон")]
        private float _damage;
        [SerializeField]
        [Tooltip("Магическая защита")]
        private float _magicArmor;
        [SerializeField]
        [Tooltip("Физическая Защита")]
        private float _physicalArmor;
        [SerializeField]
        [Tooltip("Удача")]
        private float _lacky;
        [SerializeField]
        [Tooltip("Уклонение")]
        private float _evasion;

        [Space]

        [Range(0, 100)]
        [SerializeField]
        [Tooltip("Шанс крита")]
        private float _critChance;
        
        [Range(0, 100)]
        [SerializeField]
        [Tooltip("Точность")]
        private float _accuracy;

        [SerializeField]
        private float _attackSpeed;
        
        [SerializeField]
        private float _moveSpeed = 1f;

        [Space]

        [SerializeField]
        private List<AggressionWeight> _aggressionWeights;
        [SerializeField]
        private MinionDefinition _minionDefinition;
        [SerializeField]
        private MinionLevelsCurveDefinition _minionLevelsCurve;

        [JsonIgnore]
        public int Id { get => _id; set => _id = value; }
        [JsonIgnore]
        public string Name => _name;
        [JsonIgnore]
        public string Description => _description;
        [JsonIgnore]
        public Sprite Icon => _icon;
        [JsonIgnore]
        public Rare Rare => _rare;
        [JsonIgnore]
        public int Level { get => _level; set => _level = value; }
        [JsonIgnore]
        public float CurHealth { get => _curHealth; set => _curHealth = value; }
        [JsonIgnore]
        public float MaxHealth { get => _maxHealth; set => _maxHealth = value; }
        [JsonIgnore]
        public float Damage => _damage;
        [JsonIgnore]
        public float AttackSpeed => _attackSpeed;
        [JsonIgnore]
        public float MoveSpeed => _moveSpeed;
        [JsonIgnore]
        public float CritChance => _critChance;
        [JsonIgnore]
        public float Accuracy => _accuracy;
        [JsonIgnore]
        public List<AggressionWeight> AggressionWeights => _aggressionWeights;
        [JsonIgnore]
        public MinionDefinition MinionDefinition => _minionDefinition;
        [JsonIgnore]
        public MinionLevelsCurveDefinition MinionLevelsCurve => _minionLevelsCurve;

        private void OnValidate()
        {
            _curHealth = _maxHealth;
        }


        public byte GetMinionSquadsCount()
        {
            byte result = 0;
            foreach (int addFromLevel in _minionLevelsCurve.AddFromLevel)
            {
                if (addFromLevel <= _level)
                    result++;
            }
            return result;
        }

        public string Log()
        {
            return ($"Hero ID: {_id}, Name: {_name}, Level: {_level}, Rare: {_rare}, Damage: {_damage}, MaxHealth: {_maxHealth}");
        }
    }
}
