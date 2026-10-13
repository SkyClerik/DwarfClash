using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "GameCurrencyDefinition", menuName = "Data/GameCurrency/GameCurrencyDefinition")]
    [JsonObject(MemberSerialization.OptIn)]
    public class GameCurrencyDefinition : ScriptableObject, IIdentifiable
    {
        [JsonProperty]
        [SerializeField]
        [ReadOnly]
        private int _id;
        [JsonProperty]
        [SerializeField]
        private int _curAmount;
        [JsonProperty]
        [SerializeField]
        private int _maxAmount;

        [SerializeField]
        private string _title;
        [SerializeField]
        private Sprite _image;

        [SerializeField]
        [TextArea(2,10)]
        [JsonIgnore]
        private string _devDescription;


        [JsonIgnore]
        public int Id { get => _id; set => _id = value; }
        [JsonIgnore]
        public string Title { get => _title; set => _title = value; }
        [JsonIgnore]
        public Sprite Image { get => _image; set => _image = value; }
        [JsonIgnore]
        public int CurAmount
        {
            get => _curAmount;
            set
            {
                if (_curAmount + value > int.MaxValue)
                {
                    //TODO исключение по лимиту числового значеия
                    Debug.Log("Что делать если ресурса больше чем может держать поле типа int?");
                }

                if (_curAmount != value)
                {
                    _curAmount = value;

                    if (_curAmount >= _maxAmount)
                    {
                        //TODO исключение по лимиту ресурсов
                        Debug.Log("Что делать если нет места для ресурса?");
                    }

                    RaiseCurrencyDefinitionChanged();
                }
            }
        }
        [JsonIgnore]
        public int MaxAmount
        {
            get => _maxAmount;
            set
            {
                if (_maxAmount != value)
                {
                    _maxAmount = value;
                    RaiseCurrencyDefinitionChanged();
                }
            }
        }

        public event Action OnCurrencyDefinitionChanged;

        private void RaiseCurrencyDefinitionChanged()
        {
            OnCurrencyDefinitionChanged?.Invoke();
        }
    }
}