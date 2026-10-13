using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BattlePass", menuName = "Data/Battle Pass/BattlePass Definition")]
public class BattlePassDefinition : ScriptableObject
{
    [Tooltip("Уникальный ID этого сезона, например, 'Season1_2025'")]
    public string SeasonId;
    
    [Tooltip("Список всех уровней и наград в этом сезоне")]
    public List<BattlePassTier> Tiers;

}
