using Data;
using Toolbox;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HeroDragHandler
    {
        private VisualElement _root;
        private VisualElement _dragIcon;

        private HeroBoxTreeAsset _heroBoxTreeAsset;
        private HeroDefinition _draggedHero;

        private Vector2 _localPos;

        public HeroDragHandler(VisualElement root)
        {
            _root = root;
            _dragIcon = new VisualElement();
            _dragIcon.style.position = Position.Absolute;
            _dragIcon.style.width = 100; // Можно будет настраивать
            _dragIcon.style.height = 100;
            _dragIcon.pickingMode = PickingMode.Ignore;
            _dragIcon.style.display = DisplayStyle.None;
            _root.Add(_dragIcon);
        }

        public void StartDrag(HeroBoxTreeAsset heroBoxTreeAsset, HeroDefinition hero)
        {
            _heroBoxTreeAsset = heroBoxTreeAsset;
            _draggedHero = hero;
            _dragIcon.SetBackgroundImage(_draggedHero.Icon);
            _dragIcon.SetDisplay(DisplayStyle.Flex);
        }

        public void StopDrag()
        {
            _draggedHero = null;
            _dragIcon.style.display = DisplayStyle.None;
        }

        public void UpdatePosition(Vector2 panelPosition)
        {
            if (_draggedHero == null)
                return;

            // The event position is already in the correct panel space (Y-down from top-left).
            // We just need to center the icon on the cursor.
            _dragIcon.style.left = panelPosition.x - _dragIcon.resolvedStyle.width / 2;
            _dragIcon.style.top = panelPosition.y - _dragIcon.resolvedStyle.height / 2;
        }
    }
}
