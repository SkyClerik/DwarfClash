using System;
using Data;
using Toolbox;
using UnityEngine;

public class BattlePassManager : Singleton<BattlePassManager>
{
    public event Action<int> OnXpChanged;
    public event Action<int> OnTierStateChanged;

    private PlayerDefinition _player;
    private BattlePassDefinition _currentSeason;

    // Вызывается при старте игры
    public void Initialize(PlayerDefinition player, BattlePassDefinition currentSeason)
    {
        _player = player;
        _currentSeason = currentSeason;

        // Если у игрока данные от другого сезона, сбрасываем их
        if (_player.BattlePassData.CurrentSeasonId != _currentSeason.SeasonId)
        {
            _player.BattlePassData = new PlayerBattlePassData
            {
                CurrentSeasonId = _currentSeason.SeasonId,
                HasPremiumPass = _player.BattlePassData.HasPremiumPass // Сохраняем статус премиума
            };
        }
    }

    // Вызывай этот метод, когда игрок получает опыт (например, за квест)
    public void AddXp(int amount)
    {
        if (_player == null) return;
        _player.BattlePassData.CurrentXp += amount;

        OnXpChanged?.Invoke(_player.BattlePassData.CurrentXp);
    }

    // Проверка статуса уровня для UI
    public TierStatus GetTierStatus(int tierIndex)
    {
        var tier = _currentSeason.Tiers[tierIndex];
        if (_player.BattlePassData.CurrentXp < tier.XpRequired)
        {
            return TierStatus.Locked;
        }

        bool isFreeClaimed = _player.BattlePassData.ClaimedFreeTiers.Contains(tierIndex);

        // Если нет премиума, то премиум награда считается "собранной"
        bool isPremiumClaimed = !_player.BattlePassData.HasPremiumPass || _player.BattlePassData.ClaimedPremiumTiers.Contains(tierIndex);

        if (isFreeClaimed && isPremiumClaimed)
        {
            return TierStatus.Claimed;
        }

        return TierStatus.Unlocked;
    }

    // Игрок нажимает кнопку "Забрать"
    public bool ClaimTierRewards(int tierIndex)
    {
        if (GetTierStatus(tierIndex) != TierStatus.Unlocked)
        {
            Debug.LogWarning($"Нельзя забрать награду с уровня {tierIndex}. Статус: {GetTierStatus(tierIndex)}");
            return false;
        }

        var tier = _currentSeason.Tiers[tierIndex];
        var playerData = _player.BattlePassData;

        bool claimedSomething = false;

        // Выдаем бесплатные награды
        if (!playerData.ClaimedFreeTiers.Contains(tierIndex))
        {
            foreach (var reward in tier.FreeRewards)
            {
                GiveRewardToPlayer(reward);
            }
            playerData.ClaimedFreeTiers.Add(tierIndex);
            claimedSomething = true;
        }

        // Выдаем платные награды
        if (playerData.HasPremiumPass && !playerData.ClaimedPremiumTiers.Contains(tierIndex))
        {
            foreach (var reward in tier.PremiumRewards)
            {
                GiveRewardToPlayer(reward);
            }
            playerData.ClaimedPremiumTiers.Add(tierIndex);
            claimedSomething = true;
        }

        if (claimedSomething)
        {
            Debug.Log($"Награды за уровень {tierIndex} успешно получены.");
            OnTierStateChanged?.Invoke(tierIndex);
            return true;
        }

        return false;
    }

    private void GiveRewardToPlayer(TrophyItem trophy)
    {
        // Здесь твоя логика по начислению предмета/валюты игроку.
        // У тебя уже есть `ItemDefinition` и `PlayerDefinition`, так что это будет несложно.
        // Например, найти предмет в `_player.ItemDefinitions` и увеличить его количество.
        Debug.Log($"Выдана награда: {trophy.Item.name}, количество: {trophy.GetAmount}");
    }
}

public enum TierStatus : byte
{
    Locked = 10,   // Недоступен, нужно больше опыта
    Unlocked = 20, // Доступен для сбора
    Claimed = 30   // Уже собран
}
