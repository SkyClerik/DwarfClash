using Toolbox;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace UI
{
    public class BattleTableView
    {
        private UIDocument _document;

        private VisualElement _offset;
        private const string _offsetID = "offset";
        //list
        private VisualElement _list;
        private VisualElement _heroListScrollView;
        private VisualElement _listContent;
        private VisualElement _header;
        private Button _buttonClose;
        private const string _listID = "list";
        private const string _heroListScrollViewID = "hero_list_scroll_view";
        private const string _headerID = "header";
        private const string _bCloseID = "b_close";
        //player
        private VisualElement _playerTable;
        private VisualElement _playerTableContent;
        public List<HeroBoxTreeAsset> PlayerHeroBoxes { get; } = new List<HeroBoxTreeAsset>();
        private const string _playerTableID = "player_table";
        //enemy
        private VisualElement _enemyTable;
        private VisualElement _enemyTableContent;
        private const string _enemyTableID = "enemy_table";

        private const string _contentID = "content";
        public Button ButtonClose => _buttonClose;
        public VisualElement HeroListScrollView => _heroListScrollView;
        public VisualElement ListContent => _listContent;


        public BattleTableView(UIDocument document, VisualTreeAsset heroBoxTreeAsset)
        {
            _document = document;
            _document.enabled = true;

            _offset = _document.rootVisualElement.Q(_offsetID);

            _list = _offset.Q(_listID);
            _header = _list.Q(_headerID);
            _heroListScrollView = _list.Q(_heroListScrollViewID);
            _listContent = _list.Q(_contentID);
            _listContent.pickingMode = PickingMode.Position; // Make the list area pickable
            _buttonClose = _header.Q<Button>(_bCloseID);

            _playerTable = _offset.Q(_playerTableID);
            _playerTableContent = _playerTable.Q(_contentID);

            foreach (var line in _playerTableContent.Children().ToList())
            {
                var boxesCount = line.childCount;
                line.Clear();

                for (int i = 0; i < boxesCount; i++)
                {
                    var newHeroBox = new HeroBoxTreeAsset(line, heroBoxTreeAsset);
                    PlayerHeroBoxes.Add(newHeroBox);
                }
            }

            _enemyTable = _offset.Q(_enemyTableID);
            _enemyTableContent = _enemyTable.Q(_contentID);
        }

        public void UpdateList(Player player, VisualTreeAsset heroBoxTreeAsset)
        {
            _listContent.Clear();

            // The list of heroes can be quite long, so I'll create lines of 6 heroes each.
            const int heroesPerLine = 6;
            List<Data.HeroDefinition> allHeroes = player.PlayerDefinition.Heroes;

            for (int i = 0; i < allHeroes.Count; i += heroesPerLine)
            {
                List<Data.HeroDefinition> heroesInCurrentLine = allHeroes.Skip(i).Take(heroesPerLine).ToList();
                CreateListLine(_listContent, heroBoxTreeAsset, heroesInCurrentLine, heroesPerLine);
            }
        }

        private void CreateListLine(VisualElement content, VisualTreeAsset heroBoxTreeAsset, List<Data.HeroDefinition> heroesInLine, int heroesPerLine)
        {
            var line = new VisualElement();
            line.pickingMode = PickingMode.Ignore;
            line.style.flexDirection = FlexDirection.Row;
            line.SetWidthPercentage(100);
            line.style.minHeight = 110;
            line.style.alignItems = Align.Center;
            line.style.justifyContent = Justify.Center;
            line.style.marginBottom = 4;
            line.style.marginTop = 4;
            content.Add(line);

            for (int i = 0; i < heroesPerLine; i++)
            {
                var heroBox = new HeroBoxTreeAsset(line, heroBoxTreeAsset);
                if (i < heroesInLine.Count)
                {
                    var heroDefinition = heroesInLine[i];
                    if (heroDefinition != null)
                    {
                        heroBox.SetHero(heroDefinition);
                    }
                }
                else
                {
                    heroBox.HideHero();
                }
            }
        }

        public void UpdatePlayerTable(Player player, BattleTable battleTable)
        {
            if (battleTable == null || battleTable.Table == null) return;

            int boxIndex = 0;
            foreach (var line in battleTable.Table)
            {
                foreach (var heroDefExt in line.HeroBoxes)
                {
                    if (boxIndex < PlayerHeroBoxes.Count)
                    {
                        PlayerHeroBoxes[boxIndex].SetHero(heroDefExt?.baseHeroDefenition);
                        boxIndex++;
                    }
                }
            }
        }

        public void UpdateEnemyTable(Player player, VisualTreeAsset heroBoxTreeAsset)
        {
            _enemyTableContent.Clear();
        }
    }
}

