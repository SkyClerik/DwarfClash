#define THIS_DEBUG
using Data;
using System.Collections;
using Toolbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameHudPage : MonoBehaviour
    {
        private class ResourceElementContent
        {
            private VisualElement _rootElement;
            private VisualElement _resourceContainer;
            private GameCurrencyDefinition _currencyDefinition;

            private VisualElement _image;
            private const string _imageName = "image";
            private Label _currentValue;
            private const string _currentValueName = "current_value";
            private Label _maxValue;
            private const string _maxValueName = "max_value";

            public ResourceElementContent(VisualElement rootElement, string resourceRootName, GameCurrencyDefinition gameCurrencyDefinition)
            {
                _rootElement = rootElement;
                _resourceContainer = _rootElement.Q(resourceRootName);
                _currencyDefinition = gameCurrencyDefinition;

                _image = _resourceContainer.Q(_imageName);
                _currentValue = _resourceContainer.Q<Label>(_currentValueName);
                _maxValue = _resourceContainer.Q<Label>(_maxValueName);

                SetUpdateImage();
                SetUpdateAmmount();

                _currencyDefinition.OnCurrencyDefinitionChanged += OnCurrencyDefinitionChanged;
            }

            private void OnCurrencyDefinitionChanged()
            {
                SetUpdateAmmount();
            }

            public void SetUpdateImage()
            {
                _image.style.backgroundImage = new StyleBackground(_currencyDefinition.Image);
            }

            private void SetUpdateAmmount()
            {
                _currentValue.text = $"{_currencyDefinition.CurAmount}";
                _maxValue.text = $"{_currencyDefinition.MaxAmount}";
            }

            public void Dispose()
            {
                _currencyDefinition.OnCurrencyDefinitionChanged -= OnCurrencyDefinitionChanged;
#if THIS_DEBUG
                Debug.Log($"Disposed resource element for {_currencyDefinition.name}");
#endif
            }
        }

        private PlayerDefinition _playerDefinition;
        private UIDocument _document;

        private VisualElement _topArea;
        private ResourceElementContent _resourceEnergy;
        private ResourceElementContent _resourceCristal;
        private ResourceElementContent _resourceGold;
        private const string _topAreaName = "top_area";
        private const string _resourceEnergyName = "resource_energy";
        private const string _resourceCristalName = "resource_cristal";
        private const string _resourceGoldName = "resource_gold";

        private VisualElement _buttomArea;
        private VisualElement _buttonsArea;
        private VisualElement _infoArea;
        private const string _buttomAreaName = "buttom_area"; // root element absolute
        private const string _buttonsAreaName = "buttons_area";
        private const string _infoAreaName = "info_area";

        private Button _buttonOpenHeroPage;
        private Button _buttonOpenFormPage;
        private Button _buttonOpenBuildPage;
        private Button _buttonOpenInventoryPage;
        private const string _buttonOpenHeroPageName = "button_open_hero_page";
        private const string _buttonOpenFormPageName = "button_open_form_page";
        private const string _buttonOpenBuildPageName = "button_open_build_page";
        private const string _buttonOpenInventoryPageName = "button_open_inventory_page";

        private VisualElement _battleInfoArea;
        private VisualElement _closeAreaElement;
        private Button _startBattle;
        private Button _bClose;
        private const string _battleInfoAreaID = "battle_info_area"; // root element absolute
        private const string _closeAreaElementID = "close_area_element";
        private const string _startBattleID = "start_battle";
        private const string _bCloseID = "b_close";

        private BattleTable _enemyBattleTable;

        [SerializeField]
        [Tooltip("Корневой объект для показа/скрытия сцены")]
        private GameObject _rootVisualObject;

#if UNITY_EDITOR
        [SerializeField]
        private SceneAsset _scene;
#endif
        [SerializeField]
        [ReadOnly]
        private string _battleSceneName;

        private void OnValidate()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: true);

#if UNITY_EDITOR
            _battleSceneName = _scene != null ? _scene.name : string.Empty;
#endif
        }

        private void Start()
        {
            _playerDefinition = Player.Instance.PlayerDefinition;

            MainSceneUIManager.Instance.Pages.Add(this.GetType(), this);

            Init();
        }

        private void Init()
        {
            FindTopElements();
            FindBottomElements();
            FindBattleInfoArea();
            SubscribeToButtons();
        }

        public void Show() => _document.rootVisualElement.SetDisplay(DisplayStyle.Flex);

        public void Hide() => _document.rootVisualElement.SetDisplay(DisplayStyle.None);

        public void ShowBattleInfoArea(BattleTable battleTable)
        {
            _enemyBattleTable = battleTable;
            _battleInfoArea.SetDisplay(DisplayStyle.Flex);
            _buttomArea.SetDisplay(DisplayStyle.None);

            if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage page))
                page.Hide();
        }

        public void HideBattleInfoArea()
        {
            _battleInfoArea.SetDisplay(DisplayStyle.None);
            _buttomArea.SetDisplay(DisplayStyle.Flex);

            if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage page))
                page.Show();
        }

        private bool IsBattleSceneLoaded()
        {
            Scene scene = SceneManager.GetSceneByName(_battleSceneName);
            return scene.isLoaded;
        }

        private IEnumerator LoadBattleSceneAndStartCoroutine()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_battleSceneName, LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                // Например: Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
                yield return null;
            }

            Debug.Log($"Battle scene '{_battleSceneName}' loaded. Starting battle...");
            OnButtonStartBattleClicked();

            // СКРОЙ ЭКРАН ЗАГРУЗКИ ЗДЕСЬ!
        }

        private void FindTopElements()
        {
            _topArea = _document.rootVisualElement.Q(_topAreaName);
            _resourceEnergy = new ResourceElementContent(_topArea, _resourceEnergyName, _playerDefinition.GameCurrencyCollection.Energy);
            _resourceCristal = new ResourceElementContent(_topArea, _resourceCristalName, _playerDefinition.GameCurrencyCollection.Crystals);
            _resourceGold = new ResourceElementContent(_topArea, _resourceGoldName, _playerDefinition.GameCurrencyCollection.Gold);
        }

        private void FindBottomElements()
        {
            _buttomArea = _document.rootVisualElement.Q(_buttomAreaName);
            _buttonsArea = _buttomArea.Q(_buttonsAreaName);
            _buttonOpenHeroPage = _buttonsArea.Q<Button>(_buttonOpenHeroPageName);
            _buttonOpenFormPage = _buttonsArea.Q<Button>(_buttonOpenFormPageName);
            _buttonOpenBuildPage = _buttonsArea.Q<Button>(_buttonOpenBuildPageName);
            _buttonOpenInventoryPage = _buttonsArea.Q<Button>(_buttonOpenInventoryPageName);

            _infoArea = _buttomArea.Q(_infoAreaName);
        }

        private void FindBattleInfoArea()
        {
            _battleInfoArea = _document.rootVisualElement.Q(_battleInfoAreaID);
            _closeAreaElement = _battleInfoArea.Q(_closeAreaElementID);
            _startBattle = _battleInfoArea.Q<Button>(_startBattleID);
            _bClose = _battleInfoArea.Q<Button>(_bCloseID);
        }

        private void SubscribeToButtons()
        {
            _buttonOpenHeroPage.clicked += OnButtonOpenHeroPageClicked;
            _buttonOpenFormPage.clicked += OnButtonOpenFormPageClicked;
            _buttonOpenBuildPage.clicked += OnButtonOpenBuildPageClicked;
            _buttonOpenInventoryPage.clicked += OnButtonOpenInventoryPageClicked;
            _startBattle.clicked += OnButtonStartBattleClicked;
            _bClose.clicked += HideBattleInfoArea;

            _closeAreaElement.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }

        private void UnsubscribeFromButtons()
        {
            _buttonOpenHeroPage.clicked -= OnButtonOpenHeroPageClicked;
            _buttonOpenFormPage.clicked -= OnButtonOpenFormPageClicked;
            _buttonOpenBuildPage.clicked -= OnButtonOpenBuildPageClicked;
            _buttonOpenInventoryPage.clicked -= OnButtonOpenInventoryPageClicked;
            _startBattle.clicked -= OnButtonStartBattleClicked;
            _bClose.clicked -= HideBattleInfoArea;

            _closeAreaElement.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }

        private void OnPointerUpEvent(PointerUpEvent ev)
        {
            HideBattleInfoArea();
        }

        private void OnButtonOpenHeroPageClicked()
        {
#if THIS_DEBUG
            Debug.Log("OnButtonOpenHeroPageClicked");
#endif
            Hide();

            if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage selfHudPage))
                selfHudPage.Hide();

            if (MainSceneUIManager.Instance.TryGetPage(out HeroPage heroPage))
                heroPage.Show();
        }

        private void OnButtonOpenFormPageClicked()
        {
#if THIS_DEBUG
            Debug.Log("OnButtonOpenFormPageClicked");
#endif
            Hide();
            if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage selfHudPage))
                selfHudPage.Hide();

            if (MainSceneUIManager.Instance.TryGetPage(out BattleTablePage battleTablePage))
                battleTablePage.Show();
        }

        private void OnButtonOpenBuildPageClicked()
        {
#if THIS_DEBUG
            Debug.Log("OnButtonOpenFormPageClicked");
#endif
            Hide();
            if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage selfHudPage))
                selfHudPage.Hide();

            if (MainSceneUIManager.Instance.TryGetPage(out BuildingsPage buildingsPage))
                buildingsPage.Show();
        }

        private void OnButtonOpenInventoryPageClicked()
        {
#if THIS_DEBUG
            Debug.Log("OnButtonOpenFormPageClicked");
#endif
            Hide();

            if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage selfHudPage))
                selfHudPage.Hide();
        }

        private void OnButtonStartBattleClicked()
        {
            Debug.Log("OnButtonStartBattleClicked - Checking battle scene status.");

            if (IsBattleSceneLoaded())
            {
                Debug.Log($"Battle scene '{_battleSceneName}' is already loaded. Starting battle...");
                _rootVisualObject.SetActive(false);
                HideBattleInfoArea();

                if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage selfHudPage))
                    selfHudPage.Hide();

                if (MainSceneUIManager.Instance.TryGetPage(out GameHudPage gameHudPage))
                    gameHudPage.Hide();

                BattlefieldManager.Instance.StartBattle(_playerDefinition.BattleTable, _enemyBattleTable);
            }
            else
            {
                Debug.Log($"Battle scene '{_battleSceneName}' not loaded. Loading additively...");
                // ЗДЕСЬ МОЖНО ПОКАЗАТЬ ЭКРАН ЗАГРУЗКИ!
                StartCoroutine(LoadBattleSceneAndStartCoroutine());
            }
        }

        private void OnDestroy()
        {
            _resourceEnergy?.Dispose();
            _resourceCristal?.Dispose();
            _resourceGold?.Dispose();

            UnsubscribeFromButtons();
        }
    }
}
