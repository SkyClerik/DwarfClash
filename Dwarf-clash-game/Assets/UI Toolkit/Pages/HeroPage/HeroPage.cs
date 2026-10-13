using Toolbox;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class HeroPage : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private UIDocument _document;
        [SerializeField]
        private VisualTreeAsset _heroBoxTreeAsset;

        private HeroPageView _heroPageBase;

        [SerializeField]
        private AudioClip _buttonClick;
        [SerializeField]
        private AudioClip _boxClick;

        public UIDocument GetUIDocument => _document;

        private void OnValidate()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: true);
        }

        private void Awake()
        {
            _document.enabled = true;
            _heroPageBase = new HeroPageView(_document, _heroBoxTreeAsset);

            _heroPageBase.Left.bClose.clicked += OnButtonCloseClicked;
            _heroPageBase.Left.bRecruit.clicked += OnButtonRecruitClicked;
            _heroPageBase.Left.bHeroLevelAdjust.clicked += OnButtonHeroLevelAdjustClicked;
            _heroPageBase.Left.bUnitLevelAdjust.clicked += OnButtonUnitLevelAdjustClicked;

            Hide();
        }

        private void Start()
        {
            MainSceneUIManager.Instance.Pages.Add(this.GetType(), this);
        }

        public void Show() => _document.rootVisualElement.SetDisplay(DisplayStyle.Flex);

        public void Hide() => _document.rootVisualElement.SetDisplay(DisplayStyle.None);

        private void OnButtonRecruitClicked()
        {
            Debug.Log($"Заглушка, открытие окна найма.");
        }

        private void OnButtonHeroLevelAdjustClicked()
        {
            Debug.Log($"Заглушка, кнопка прокачки героя.");
        }

        private void OnButtonUnitLevelAdjustClicked()
        {
            Debug.Log($"Заглушка, кнопка прокачки юнита.");
        }

        private void OnButtonCloseClicked()
        {
            Hide();

            if (MainSceneUIManager.Instance.TryGetPage(out SelfHudPage selfHudPage))
                selfHudPage.Show();

            if (MainSceneUIManager.Instance.TryGetPage(out GameHudPage gameHudPage))
                gameHudPage.Show();
        }
    }
}
