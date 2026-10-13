using Newtonsoft.Json;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "MinionDefinition", menuName = "Data/MinionDefinition")]
    public class MinionDefinition : ScriptableObject, IIdentifiable
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
        private Rare _rare;

        [SerializeField, Space]
        private float _curHealth;
        [SerializeField]
        private float _maxHealth;
        [SerializeField]
        private float _damage;
        [SerializeField]
        private float _attackSpeed;

        [SerializeField, Space]
        private float _moveSpeed = 1f;
        [SerializeField]
        private float _armor;
        [SerializeField]
        private float _penetrationArmor;

        [Range(0, 100)]
        [SerializeField, Space]
        private float _critChance;
        [Range(0, 100)]
        [SerializeField]
        private float _accuracy;
        [SerializeField]
        private float _targetUpdateSpeed = 0.5f;

        [SerializeField]
        private Texture _spriteAtlas;


        [JsonIgnore]
        public int Id { get => _id; set => _id = value; }
        public string Name => _name;
        public string Description => _description;
        public Sprite Icon => _icon;
        public Rare Rare => _rare;
        public float CurHealth { get => _curHealth; set => _curHealth = value; }
        public float MaxHealth { get => _maxHealth; set => _maxHealth = value; }
        public float Damage => _damage;
        public float AttackSpeed => _attackSpeed;
        public float MoveSpeed => _moveSpeed;
        public float Armor => _armor;
        public float PenetrationArmor => _penetrationArmor;
        public float CritChance => _critChance;
        public float Accuracy => _accuracy;
        public float TragetUpdateSpeed => _targetUpdateSpeed;
        public Texture SpriteAtlas => _spriteAtlas;
    }
}
