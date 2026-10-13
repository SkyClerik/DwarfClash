using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class WorldBillboardsPage : UIPage<WorldBillboardsPage>
    {
        [SerializeField]
        private VisualTreeAsset _billboardTemplate;
        [SerializeField]
        private int _totalBillboards = 10;
        [SerializeField]
        private Camera _camera;

        private List<Billboard> _billboards = new List<Billboard>();

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void Init()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Billboard billboardCash;
            for (int i = 0; i < _totalBillboards; i++)
            {
                billboardCash = CreateBillboard();
                billboardCash.style.display = DisplayStyle.None;
            }
        }

        private void FixedUpdate()
        {
            foreach (Billboard billboard in _billboards)
            {
                if (billboard.GetSceneObject == null)
                    continue;

                Relocation(billboard);

                if (billboard.style.display == DisplayStyle.Flex)
                    billboard.Tick();
            }
        }

        public void BillboardShow(GameObject target, Sprite icon, TimeSpan timerTime, Action billboardTimeUp)
        {
            Billboard freeBillboard = GetFreeBillboardFrom();
            freeBillboard.Reset(target, timerTime, billboardTimeUp);
            Relocation(freeBillboard);
            freeBillboard.SetImage(icon);
        }

        public void Relocation(Billboard billboard)
        {
            billboard.CanvasPosition = _camera.WorldToScreenPoint(billboard.GetSceneObject.transform.position);
            billboard.CanvasPosition.y = (Screen.height - billboard.CanvasPosition.y);
            var panelLocalPosition = RuntimePanelUtils.ScreenToPanel(GetDocument.rootVisualElement.panel, billboard.CanvasPosition);
            billboard.style.top = panelLocalPosition.y;
            billboard.style.left = panelLocalPosition.x;
        }

        public Billboard GetFreeBillboardFrom()
        {
            for (int i = 0; i < _billboards.Count; i++)
            {
                if (_billboards[i].style.display == DisplayStyle.None)
                {
                    return _billboards[i];
                }
            }

            var newBillboard = CreateBillboard();
            newBillboard.style.display = DisplayStyle.Flex;
            return newBillboard;
        }

        private Billboard CreateBillboard()
        {
            var newBillboard = new Billboard();
            newBillboard.Init(_billboardTemplate, GetDocument.rootVisualElement);
            _billboards.Add(newBillboard);
            return newBillboard;
        }
    }
}
