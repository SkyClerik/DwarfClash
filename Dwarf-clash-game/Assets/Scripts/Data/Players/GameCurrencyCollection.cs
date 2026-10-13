using Newtonsoft.Json;
using UnityEngine;

namespace Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "GameCurrencyCollection", menuName = "Data/GameCurrency/GameCurrencyCollection")]
    [JsonObject(MemberSerialization.OptIn)]
    public class GameCurrencyCollection : ScriptableObject
    {
        [JsonProperty]
        [SerializeField]
        [Tooltip("«олото Ч постройка/улучшение зданий, прокачка героев и юнитов")]
        private GameCurrencyDefinition _gold;
        [JsonProperty]
        [SerializeField]
        [Tooltip("јдамантий Ч улучшение/создание предметов(только из шахты)")]
        private GameCurrencyDefinition _adamantite;
        [JsonProperty]
        [SerializeField]
        [Tooltip(" ристаллы Ч премиум-валюта(донат, разломы, шахта)")]
        private GameCurrencyDefinition _crystals;
        [JsonProperty]
        [SerializeField]
        [Tooltip("—еребро Ч улучшение армии(сюжет, разломы, квесты, постройки)")]
        private GameCurrencyDefinition _silver;
        [JsonProperty]
        [SerializeField]
        [Tooltip("«елье опыта Ч повышение уровн€ героев(сюжет, разломы, квесты, постройки)")]
        private GameCurrencyDefinition _experiencePotion;
        [JsonProperty]
        [SerializeField]
        [Tooltip("≈да Ч повышение уровн€ героев(постройки)")]
        private GameCurrencyDefinition _food;
        [JsonProperty]
        [SerializeField]
        [Tooltip("Ёнерги€ Ч валюта дл€ участи€ в битвах")]
        private GameCurrencyDefinition _energy;

        [JsonIgnore]
        public GameCurrencyDefinition Gold { get => _gold; set => _gold = value; }
        [JsonIgnore]
        public GameCurrencyDefinition Adamantite { get => _adamantite; set => _adamantite = value; }
        [JsonIgnore]
        public GameCurrencyDefinition Crystals { get => _crystals; set => _crystals = value; }
        [JsonIgnore]
        public GameCurrencyDefinition Silver { get => _silver; set => _silver = value; }
        [JsonIgnore]
        public GameCurrencyDefinition ExperiencePotion { get => _experiencePotion; set => _experiencePotion = value; }
        [JsonIgnore]
        public GameCurrencyDefinition Food { get => _food; set => _food = value; }
        [JsonIgnore]
        public GameCurrencyDefinition Energy { get => _energy; set => _energy = value; }
    }
}