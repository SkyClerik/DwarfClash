using Data;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "MinionLevelsCurveDefinition", menuName = "Data/MinionLevelsCurveDefinition")]
public class MinionLevelsCurveDefinition : ScriptableObject
{
    [SerializeField]
    [Range(1, 4)]
    private byte _unitsInSquad;
    [SerializeField]
    private List<int> _addFromLevel;

    public List<int> AddFromLevel { get => _addFromLevel; set => _addFromLevel = value; }
    public byte UnitsInSquad { get => _unitsInSquad; set => _unitsInSquad = value; }

    public MiniSquadFormation GetMiniSquadFormation(HeroDefinition heroDefinition)
    {
        byte maxMiniSquades = heroDefinition.GetMinionSquadsCount();

        if (maxMiniSquades % 2 == 0)
            return MiniSquadFormation.EvenNumbers;
        else
            return MiniSquadFormation.NotEvenNumbers;
    }
}

public enum MiniSquadFormation : byte
{
    EvenNumbers,
    NotEvenNumbers
}
