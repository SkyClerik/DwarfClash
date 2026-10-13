using UnityEngine.UIElements;

namespace UI
{
    public class HeroPageView
    {
        private UIDocument _document;
        private VisualTreeAsset _heroBoxTreeAsset;

        public struct LeftWindow
        {
            private VisualElement _root;
            private const string _rootID = "left_window";

            private VisualElement _header;
            private VisualElement _body;
            private VisualElement _adjust;
            private const string _headerID = "header";
            private const string _bodyID = "body";
            private const string _adjustID = "adjust";

            //header
            private Button _bClose;
            private Button _bRecruit;
            private const string _bCloseID = "b_close";
            private const string _bRecruitID = "b_recruit";

            //body
            private VisualElement _heroListContent;
            private const string _heroListContentID = "hero_list_content";

            //update
            private Button _bHeroLevelAdjust;
            private Button _bUnitLevelAdjust;
            private const string _bHeroLevelAdjustID = "b_hero_level_adjust";
            private const string _bUnitLevelAdjustID = "b_unit_level_adjust";

            public Button bClose => _bClose;
            public Button bRecruit => _bRecruit;
            public Button bHeroLevelAdjust => _bHeroLevelAdjust;
            public Button bUnitLevelAdjust => _bUnitLevelAdjust;

            public void Init(VisualElement rootVisualElement)
            {
                _root = rootVisualElement.Q(_rootID);

                _header = _root.Q(_headerID);
                _body = _root.Q(_bodyID);
                _adjust = _root.Q(_adjustID);

                //header
                _bClose = _header.Q<Button>(_bCloseID);
                _bRecruit = _header.Q<Button>(_bRecruitID);

                //body
                _heroListContent = _body.Q(_heroListContentID);

                //adjust
                _bHeroLevelAdjust = _adjust.Q<Button>(_bHeroLevelAdjustID);
                _bUnitLevelAdjust = _adjust.Q<Button>(_bUnitLevelAdjustID);
            }

            public void UpdateHeroList(Player player, VisualTreeAsset heroBoxTreeAsset)
            {
                _heroListContent.Clear();

                foreach (Data.HeroDefinition heroDefinition in player.PlayerDefinition.Heroes)
                {
                    var heroBox = new HeroBoxTreeAsset(_heroListContent, heroBoxTreeAsset);
                    heroBox.SetHero(heroDefinition);
                }
            }
        }

        public struct RightWindow
        {
            private VisualElement _root;
            private const string _rootID = "right_window";

            private VisualElement _header;
            private VisualElement _heroInfoArea;
            private VisualElement _unitsInfoArea;
            private VisualElement _equipmentArea;
            private const string _headerID = "header";
            private const string _heroInfoAreaID = "hero_info_area";
            private const string _unitsInfoAreaID = "units_info_area";
            private const string _equipmentAreaID = "equipment_area";

            //hero_info_area


            //units_info_area


            //equipment_area

        }

        private LeftWindow _leftWindow = new LeftWindow();
        private RightWindow _rightWindow = new RightWindow();

        public LeftWindow Left => _leftWindow;
        public RightWindow Right => _rightWindow;

        public HeroPageView(UIDocument document, VisualTreeAsset heroBoxTreeAsset)
        {
            _document = document;
            _heroBoxTreeAsset = heroBoxTreeAsset;
            _leftWindow.Init(_document.rootVisualElement);
        }
    }
}
