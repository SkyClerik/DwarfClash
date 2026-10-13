using Data;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "BattleTable", menuName = "Data/BattleTable")]
[JsonObject(MemberSerialization.OptIn)]
public class BattleTable : ScriptableObject
{
    [System.Serializable]
    public class HeroTableLine
    {
        [System.Serializable]
        public class HeroDefenitionExtension
        {
            [Tooltip("Базовые данные героя (Шаблон)")]
            public HeroDefinition baseHeroDefenition;
            [Tooltip("Переопределение значения шаблонного уровня (Это значение подставится в шаблон при запросе копии)")]
            public int redefinedLevel = 1;
        }

        [SerializeField]
        private HeroDefenitionExtension[] _heroBoxes = new HeroDefenitionExtension[5];
        public HeroDefenitionExtension[] HeroBoxes => _heroBoxes;
    }

    [SerializeField]
    private HeroTableLine[] _table = new HeroTableLine[7];
    public HeroTableLine[] Table => _table;
}
