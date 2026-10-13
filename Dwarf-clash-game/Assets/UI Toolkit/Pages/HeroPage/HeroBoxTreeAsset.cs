using Data;
using Toolbox;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HeroBoxTreeAsset
    {
        private const string _heroBoxRootId = "hero_box";
        private const string _backgroundId = "background";
        private const string _panelId = "panel";
        private const string _imageId = "image";
        private const string _rootInfoId = "root_info";
        private const string _leftId = "left";
        private const string _rightId = "right";
        private const string _circleId = "circle";
        private const string _circleLabelId = "l_text";
        private const string _unitNameId = "unit_name";

        private VisualElement _background;
        private VisualElement _panel;
        private VisualElement _image;
        private VisualElement _rootInfo;
        private VisualElement _left;
        private VisualElement _right;
        private VisualElement _leftCircle;
        private VisualElement _rightCircle;
        private Label _leftCircleLable;
        private Label _rightCircleLable;
        private Label _unitNameLabel;

        private HeroDefinition _heroDefinition;
        private VisualElement _root;
        public VisualElement Root { get { return _root; } private set { _root = value; } } // Public property for external access

        public HeroBoxTreeAsset(VisualElement parent, VisualTreeAsset template)
        {
            _root = template.CloneTree();
            parent.Add(_root);

            _background = _root.Q(_heroBoxRootId);
            _panel = _root.Q(_panelId);
            _image = _root.Q(_imageId);
            _rootInfo = _root.Q(_rootInfoId);
            _left = _rootInfo.Q(_leftId);
            _right = _rootInfo.Q(_rightId);
            _unitNameLabel = _rootInfo.Q<Label>(_unitNameId);

            _leftCircle = _left.Q(_circleId);
            _rightCircle = _right.Q(_circleId);

            _leftCircleLable = _leftCircle.Q<Label>(_circleLabelId);
            _rightCircleLable = _rightCircle.Q<Label>(_circleLabelId);

            _image.SetDisplay(DisplayStyle.None);
            _rootInfo.SetDisplay(DisplayStyle.None);

            _root.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _root.style.marginTop = 10;
            _root.style.marginBottom = 10;
            _root.style.marginLeft = 5;
            _root.style.marginRight = 5;
            _root.SetWidthPercentage(100);
            _root.SetHeightPercentage(100);
            _root.pickingMode = PickingMode.Position;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_heroDefinition != null)
            {
                // Simply "shout into the void" that we have been pressed.
                // The central controller will pick this up.
                StaticDragEvent.HeroPointerDown(evt, this);
            }
        }

        public HeroDefinition GetHeroDefinition()
        {
            return _heroDefinition;
        }

        public void SetHero(HeroDefinition heroDefinition)
        {
            _heroDefinition = heroDefinition;

            if (heroDefinition != null)
            {
                _image.style.backgroundImage = new StyleBackground(heroDefinition.Icon);
                _unitNameLabel.text = heroDefinition.Name;
                _leftCircleLable.text = $"{heroDefinition.Level}";
                _rightCircleLable.text = $"{heroDefinition.Level}";

                ShowHero();
            }
            else
            {
                HideHero();
            }
        }

        public void ShowHero()
        {
            _image.SetDisplay(DisplayStyle.Flex);
            _rootInfo.SetDisplay(DisplayStyle.Flex);
        }

        public void HideHero()
        {
            _image.SetDisplay(DisplayStyle.None);
            _rootInfo.SetDisplay(DisplayStyle.None);
        }
    }
}
