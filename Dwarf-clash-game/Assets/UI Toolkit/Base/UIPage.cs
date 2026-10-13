using Toolbox;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.UI
{
    public class UIPage<T> : Singleton<T> where T : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        private UIDocument _document;

        public UIDocument GetDocument => _document;

        private void OnValidate()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: true);
        }

        public virtual void Show()
        {
            UserInterfaceShare.Instance.PageOpen(_document);
            _document.rootVisualElement.SetDisplay(DisplayStyle.Flex);
        }

        public virtual void Hide()
        {
            _document.rootVisualElement.SetDisplay(DisplayStyle.None);
        }
    }
}
