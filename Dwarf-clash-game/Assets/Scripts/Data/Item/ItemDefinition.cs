using Newtonsoft.Json;
using UnityEngine;

namespace Data
{
    public enum InsertType : byte
    {
        None = 0,
        Chest = 1,
        Equip = 3,
        Other = 4,
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "New ItemDefinition", menuName = "Data/ItemDefinition")]
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemDefinition : ObjectBase, IIdentifiable
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private int _id;

        [JsonProperty]
        [SerializeField]
        private int _curAmount;
        [SerializeField]
        private int _price;
        [SerializeField]
        private InsertType _type;

        [JsonIgnore]
        public int Id { get => _id; set => _id = value; }
        [JsonIgnore]
        public int Price => _price;
        [JsonIgnore]
        public int CurAmount { get => _curAmount; set => _curAmount = value; }
        [JsonIgnore]
        public InsertType Type => _type;

        public void PlusAmount(TrophyItem trophyItem)
        {
            var value = _curAmount + trophyItem.GetAmount;
            if (value > MaxAmount)
            {
                Debug.Log($"Есть избыток {this.name}: CurPCS:{_curAmount} - MaxPCS:{MaxAmount}");
                _curAmount = MaxAmount;

                ItemDefinition remainder = Instantiate(this);
                remainder.CurAmount = value - MaxAmount;
                return;
            }
            else
            {
                _curAmount = value;
                if (value < MaxAmount) Debug.Log($"Просто добавляем {value}");
                return;
            }
        }

        public void MinusAmount(TrophyItem trophyItem)
        {
            var value = _curAmount - trophyItem.GetAmount;
            _curAmount = Mathf.Max(value, MinAmount);
            Debug.Log($"Уменьшили количество {this.name} до {_curAmount}");
        }
    }

    [System.Serializable]
    public class TrophyItem
    {
        [SerializeField]
        private ItemDefinition _item;

        [SerializeField]
        private IntMinMax _minMaxAmount;
        private int _resultAmount;

        [Range(0f, 100f), SerializeField]
        private byte _chance;

        public ItemDefinition Item => _item;
        public int GetAmount
        {
            get
            {
                var rValue = UnityEngine.Random.Range(_minMaxAmount.Min, _minMaxAmount.Max);
                return _resultAmount = _minMaxAmount.OnlyMin ? _minMaxAmount.Min : rValue;
            }
        }

        public byte Chance => _chance;
    }

    [System.Serializable]
    public struct IntMinMax
    {
        public bool OnlyMin;
        public int Min;
        public int Max;
    }
}