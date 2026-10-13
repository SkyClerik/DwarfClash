using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "PlayerDefinition", menuName = "Data/PlayerDefinition")]
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayerDefinition : ScriptableObject
    {
        [JsonProperty]
        [SerializeField]
        private GameCurrencyCollection _gameCurrency;
        [JsonProperty]
        [SerializeField]
        private List<HeroDefinition> _heroes = new List<HeroDefinition>();
        [JsonProperty]
        [SerializeField]
        private List<ItemDefinition> _items = new List<ItemDefinition>();

        [JsonProperty]
        [SerializeField]
        private BattleTable _battleTable;

        [JsonIgnore]
        public GameCurrencyCollection GameCurrencyCollection => _gameCurrency;
        [JsonIgnore]
        public List<ItemDefinition> Items => _items;
        [JsonIgnore]
        public List<HeroDefinition> Heroes { get => _heroes; set => _heroes = value; }
        [JsonIgnore]
        public BattleTable BattleTable => _battleTable;

        [JsonProperty]
        [SerializeField]
        private PlayerBattlePassData _battlePassData = new PlayerBattlePassData();
    
        [JsonIgnore]
        public PlayerBattlePassData BattlePassData { get => _battlePassData; set => _battlePassData = value; }

        public string ToJson()
        {
            // Formatting.Indented для читаемости (опционально)
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void FromJson(string json)
        {
            JsonConvert.PopulateObject(json, this);
        }
    }
}
