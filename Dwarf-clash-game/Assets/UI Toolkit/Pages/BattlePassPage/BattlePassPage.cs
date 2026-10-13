using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.UIElements;
using Toolbox;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class BattlePassPage : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private VisualTreeAsset _rewardTierTemplate;

        [Header("Data")]
        [SerializeField] private BattlePassDefinition _currentSeason; // Assign this in the inspector
        [SerializeField] private PlayerDefinition _player; // Assign this in the inspector

        private ScrollView _rewardsContainer;
        private BattlePassManager _battlePassManager;

        // Caching UI elements for each tier to update them later
        private readonly Dictionary<int, VisualElement> _tierElements = new Dictionary<int, VisualElement>();

        private event Action _claimTierRewards;

        private Button _claimButton;

        private void OnValidate()
        {
            _uiDocument = GetComponentInChildren<UIDocument>(includeInactive: true);
        }

        private void Awake()
        {
            _uiDocument.enabled = true;
            _rewardsContainer = _uiDocument.rootVisualElement.Q<ScrollView>("reward_scrollview");
            _uiDocument.rootVisualElement.SetDisplay(DisplayStyle.None);

            _battlePassManager = BattlePassManager.Instance;
            _battlePassManager.Initialize(_player, _currentSeason);
        }

        private void OnEnable()
        {
            RenderAllTiers();
            _battlePassManager.OnTierStateChanged += HandleTierStateChanged;
        }

        private void OnDestroy()
        {
            _claimButton.clicked -= _claimTierRewards;
            _claimTierRewards = null;
        }

        private void OnDisable()
        {
            _battlePassManager.OnTierStateChanged -= HandleTierStateChanged;
        }

        private void RenderAllTiers()
        {
            _rewardsContainer.Clear();
            _tierElements.Clear();

            if (_currentSeason == null || _currentSeason.Tiers == null)
            {
                Debug.LogError("Current season or its tiers are not set!");
                return;
            }

            for (int i = 0; i < _currentSeason.Tiers.Count; i++)
            {
                var tierData = _currentSeason.Tiers[i];
                var tierElement = _rewardTierTemplate.Instantiate();

                // Populate the tier UI
                tierElement.Q<Label>("tier-level-label").text = (i + 1).ToString();

                // --- Free Reward ---
                var freeRewardContainer = tierElement.Q("free-reward-container");
                if (tierData.FreeRewards != null && tierData.FreeRewards.Any())
                {
                    var freeReward = tierData.FreeRewards.First();
                    // NOTE: You need a way to get an icon from an ItemDefinition
                    // For now, it just sets the background color
                    freeRewardContainer.Q("free-reward-icon").style.backgroundImage = new StyleBackground(freeReward.Item.Icon);
                    freeRewardContainer.Q<Label>("free-reward-amount").text = $"x{freeReward.GetAmount}";
                }
                else
                {
                    freeRewardContainer.style.visibility = Visibility.Hidden;
                }

                // --- Premium Reward ---
                var premiumRewardContainer = tierElement.Q("premium-reward-container");
                if (tierData.PremiumRewards != null && tierData.PremiumRewards.Any())
                {
                    var premiumReward = tierData.PremiumRewards.First();
                    premiumRewardContainer.Q("premium-reward-icon").style.backgroundImage = new StyleBackground(premiumReward.Item.Icon);
                    premiumRewardContainer.Q<Label>("premium-reward-amount").text = $"x{premiumReward.GetAmount}";
                }
                else
                {
                    premiumRewardContainer.style.visibility = Visibility.Hidden;
                }

                _claimButton = tierElement.Q<Button>("claim-button");
                int tierIndex = i; // Capture index for the lambda

                _claimTierRewards = () => _battlePassManager.ClaimTierRewards(tierIndex);
                _claimButton.clicked += _claimTierRewards;

                _rewardsContainer.Add(tierElement);
                _tierElements[tierIndex] = tierElement;

                // Set initial state
                UpdateTierVisualState(tierIndex);
            }
        }

        private void HandleTierStateChanged(int tierIndex)
        {
            UpdateTierVisualState(tierIndex);
        }

        private void UpdateTierVisualState(int tierIndex)
        {
            if (!_tierElements.ContainsKey(tierIndex)) return;

            var tierElement = _tierElements[tierIndex];
            var claimButton = tierElement.Q<Button>("claim-button");
            var tierStatus = _battlePassManager.GetTierStatus(tierIndex);

            switch (tierStatus)
            {
                case TierStatus.Locked:
                    claimButton.text = "Locked";
                    claimButton.SetEnabled(false);
                    break;
                case TierStatus.Unlocked:
                    claimButton.text = "Claim";
                    claimButton.SetEnabled(true);
                    break;
                case TierStatus.Claimed:
                    claimButton.text = "Claimed";
                    claimButton.SetEnabled(false);
                    // Optional: Add a "claimed" class to the root for styling
                    tierElement.AddToClassList("claimed");
                    break;
            }
        }
    }
}
