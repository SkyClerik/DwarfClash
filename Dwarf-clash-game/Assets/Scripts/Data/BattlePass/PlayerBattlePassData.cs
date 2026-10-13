using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class PlayerBattlePassData
{
    [JsonProperty]
    public string CurrentSeasonId;

    [JsonProperty]
    public int CurrentXp;

    [JsonProperty]
    public HashSet<int> ClaimedFreeTiers = new HashSet<int>();

    [JsonProperty]
    public HashSet<int> ClaimedPremiumTiers = new HashSet<int>();


    [JsonProperty]
    public bool HasPremiumPass = false;
    
}
