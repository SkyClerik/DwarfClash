using System.Collections.Generic;
using Data;
using UnityEngine;


[System.Serializable]
public class BattlePassTier
{
    [Tooltip("Сколько опыта нужно для разблокировки уровня")]
    public int XpRequired;

    [Tooltip("Награды, доступные всем игрокам")]
    public List<TrophyItem> FreeRewards;

    [Tooltip("Награды, купившые платный пропуск")]
    public List<TrophyItem> PremiumRewards;
}
