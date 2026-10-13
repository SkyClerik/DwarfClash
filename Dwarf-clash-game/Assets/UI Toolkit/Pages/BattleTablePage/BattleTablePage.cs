using Data;
using System.Linq;
using System.Xml.Linq;
using Toolbox;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class BattleTablePage : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private UIDocument _document;
        [SerializeField]
        private VisualTreeAsset _heroBoxTreeAsset;
        private BattleTable _battleTable;

        private BattleTableView _battleTableView;
        private Player _player;

        private HeroDragHandler _heroDragHandler;

        // --- Drag & Drop State ---
        private HeroBoxTreeAsset _sourceHeroBox;
        private HeroDefinition _draggedHeroDefinition;
        private VisualElement _capturedElement;
        private Vector2 _startDragPosition;
        private bool _isDragging;
        private bool _isSourceFromTable;
        private int _sourceHeroBoxIndex;
        // -------------------------

        [SerializeField]
        private AudioClip _buttonClick;
        [SerializeField]
        private AudioClip _boxClick;

        private void OnValidate()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: true);
        }

        private void Start()
        {
            _battleTableView = new BattleTableView(_document, _heroBoxTreeAsset);
            _player = Player.Instance;
            _battleTable = _player.PlayerDefinition.BattleTable;
            _battleTableView.ButtonClose.clicked += ButtonCloseClicked;
            _heroDragHandler = new HeroDragHandler(_document.rootVisualElement);
            MainSceneUIManager.Instance.Pages.Add(this.GetType(), this);
            Hide();
        }

        private void OnHeroPointerDown(PointerDownEvent evt, HeroBoxTreeAsset heroBox)
        {
            if (_capturedElement != null) return;

            _capturedElement = heroBox.Root; // Capture the root element
            _sourceHeroBox = heroBox;
            _draggedHeroDefinition = heroBox.GetHeroDefinition();
            _startDragPosition = evt.position;
            _isDragging = false;

            _sourceHeroBoxIndex = _battleTableView.PlayerHeroBoxes.IndexOf(_sourceHeroBox);
            _isSourceFromTable = _sourceHeroBoxIndex != -1;

            _capturedElement.CapturePointer(evt.pointerId);
            _capturedElement.RegisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
            _capturedElement.RegisterCallback<PointerUpEvent>(OnGlobalPointerUp);
        }

        private void OnGlobalPointerMove(PointerMoveEvent evt)
        {
            if (_capturedElement == null || !evt.isPrimary) return;

            if (_isDragging)
            {
                _heroDragHandler.UpdatePosition(evt.position);
                return;
            }

            if (Vector2.Distance(evt.position, _startDragPosition) > 20f)
            {
                _isDragging = true;
                _heroDragHandler.StartDrag(_sourceHeroBox, _draggedHeroDefinition);
                _sourceHeroBox.HideHero();
            }
        }

        private void OnGlobalPointerUp(PointerUpEvent evt)
        {
            if (_capturedElement == null || !evt.isPrimary) return;

            // Unregister and release pointer immediately
            _capturedElement.UnregisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
            _capturedElement.UnregisterCallback<PointerUpEvent>(OnGlobalPointerUp);
            _capturedElement.ReleasePointer(evt.pointerId);

            if (!_isDragging)
            {
                // On a simple click, just show the hero again.
                _sourceHeroBox.ShowHero();
            }
            else
            {
                // On drop, execute the logic
                _heroDragHandler.StopDrag();
                VisualElement pickedElement = _document.rootVisualElement.panel.Pick(evt.position);

                // --- Target Identification ---
                HeroBoxTreeAsset targetBox = _battleTableView.PlayerHeroBoxes.FirstOrDefault(box => pickedElement != null && (box.Root == pickedElement || box.Root.Contains(pickedElement)));

                bool isTargetList = false;
                if (pickedElement != null)
                {
                    VisualElement current = pickedElement;
                    while (current != null)
                    {
                        if (current == _battleTableView.HeroListScrollView)
                        {
                            isTargetList = true;
                            break;
                        }
                        current = current.parent;
                    }
                }

                // --- Logic Execution ---
                bool dropHandled = false;

                // Case 1: Dropped on a table slot
                if (targetBox != null && targetBox != _sourceHeroBox)
                {
                    int targetBoxIndex = _battleTableView.PlayerHeroBoxes.IndexOf(targetBox);

                    if (_isSourceFromTable) // Table -> Table
                    {
                        // Get hero from target ONCE.
                        var heroInTargetExt = GetHeroDefFromTable(targetBoxIndex);
                        var heroInTargetDef = heroInTargetExt?.baseHeroDefenition;

                        // DEBUG LOG
                        Debug.Log($"--- DRAG DEBUG: Table->Table ---");
                        Debug.Log($"Source Index: {_sourceHeroBoxIndex} | Target Index: {targetBoxIndex}");
                        Debug.Log("Dragging Hero:");
                        _draggedHeroDefinition?.Log();
                        Debug.Log("Hero currently in Target Slot:");
                        if (heroInTargetDef != null)
                        {
                            heroInTargetDef.Log();
                        }
                        else
                        {
                            Debug.Log("NULL");
                        }
                        Debug.Log($"---------------------------------");
                        // END DEBUG LOG
                        
                        // Perform the swap using the cached variable 'heroInTargetDef'
                        SetHeroDefInTable(_sourceHeroBoxIndex, heroInTargetDef);
                        SetHeroDefInTable(targetBoxIndex, _draggedHeroDefinition);
                        _sourceHeroBox.SetHero(heroInTargetDef);
                        targetBox.SetHero(_draggedHeroDefinition);
                    }
                    else // List -> Table
                    {
                        var targetHeroExt = GetHeroDefFromTable(targetBoxIndex);
                        _player.PlayerDefinition.Heroes.Remove(_draggedHeroDefinition);
                        if (targetHeroExt?.baseHeroDefenition != null)
                        {
                            _player.PlayerDefinition.Heroes.Add(targetHeroExt.baseHeroDefenition);
                        }
                        SetHeroDefInTable(targetBoxIndex, _draggedHeroDefinition);
                        targetBox.SetHero(_draggedHeroDefinition);
                        _battleTableView.UpdateList(_player, _heroBoxTreeAsset);
                    }
                    dropHandled = true;
                }
                // Case 2: Dropped on the hero list area
                else if (isTargetList)
                {
                    if (_isSourceFromTable) // Table -> List
                    {
                        SetHeroDefInTable(_sourceHeroBoxIndex, null);
                        if (!_player.PlayerDefinition.Heroes.Contains(_draggedHeroDefinition))
                        {
                            _player.PlayerDefinition.Heroes.Add(_draggedHeroDefinition);
                        }
                        _sourceHeroBox.SetHero(null);
                        _battleTableView.UpdateList(_player, _heroBoxTreeAsset);
                    }
                    // For List -> List, we do nothing, which is a "cancel" but it's handled.
                    dropHandled = true;
                }

                // Case 3: Dropped in the void (Cancel) or on itself
                if (!dropHandled)
                {
                    _sourceHeroBox.ShowHero();
                }
            }

            // Cleanup state variables
            _capturedElement = null;
            _sourceHeroBox = null;
            _draggedHeroDefinition = null;
            _isDragging = false;
            _isSourceFromTable = false;
            _sourceHeroBoxIndex = -1;
        }

        // --- Data Model Helper Methods ---
        private BattleTable.HeroTableLine.HeroDefenitionExtension GetHeroDefFromTable(int index)
        {
            int currentIndex = 0;
            foreach (var line in _battleTable.Table)
            {
                foreach (var heroDefExt in line.HeroBoxes)
                {
                    if (currentIndex == index) return heroDefExt;
                    currentIndex++;
                }
            }
            return null;
        }

        private void SetHeroDefInTable(int index, HeroDefinition heroDef)
        {
            int currentIndex = 0;
            foreach (var line in _battleTable.Table)
            {
                for (int i = 0; i < line.HeroBoxes.Length; i++)
                {
                    if (currentIndex == index)
                    {
                        if (line.HeroBoxes[i] == null)
                        {
                            line.HeroBoxes[i] = new BattleTable.HeroTableLine.HeroDefenitionExtension();
                        }
                        line.HeroBoxes[i].baseHeroDefenition = heroDef;
                        // TODO: Set redefinedLevel if needed
                        return;
                    }
                    currentIndex++;
                }
            }
        }

        private void ButtonCloseClicked()
        {
            Hide();

            if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage selfHudPage))
                selfHudPage.Show();

            if (MainSceneUIManager.Instance.TryGetPage(out GameHudPage gameHudPage))
                gameHudPage.Show();
        }

        public void Show()
        {
            if (_battleTable == null)
            {
                Debug.LogError("ОШИБКА: BattleTable не назначен в инспекторе для PlayerDefinition! " +
                               "Пожалуйста, перетащите ассет BattleTable.asset в поле 'Battle Table' компонента PlayerDefinition.");
                return;
            }

            // Subscribe only to the initial static event
            StaticDragEvent.OnHeroPointerDown += OnHeroPointerDown;

            _battleTableView.UpdateList(_player, _heroBoxTreeAsset);
            _battleTableView.UpdatePlayerTable(_player, _battleTable);
            _battleTableView.UpdateEnemyTable(_player, _heroBoxTreeAsset);
            _document.rootVisualElement.SetDisplay(DisplayStyle.Flex);
        }

        public void Hide()
        {
            // Unsubscribe only from the initial static event
            StaticDragEvent.OnHeroPointerDown -= OnHeroPointerDown;

            _document.rootVisualElement.SetDisplay(DisplayStyle.None);
        }

        private void OnDestroy()
        {
            _battleTableView.ButtonClose.clicked -= ButtonCloseClicked;

            // Ensure we unsubscribe if the object is destroyed while visible
            StaticDragEvent.OnHeroPointerDown -= OnHeroPointerDown;
        }
    }
}

