using System.Collections;
using Toolbox;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SelfHudPage : MonoBehaviour
    {
        private UIDocument _document;

        private const string _moveObjectName = "move_object"; // Имя элемента UI, который будет перемещаться.
        private VisualElement _moveObject;

        private const string _buttonOpenCloseName = "button_oc"; // Непосредственно кнопка для открытия и закрытия окна.
        private Button _buttonOpenClose;

        private const string _rootWindowID = "root_window"; // Имя элемента на ширину которого будем перемещаться
        private VisualElement _rootWindow;

        //private const string _closeAreaElementID = "close_area_element";
        //private VisualElement _closeAreaElement;

        private float _closedLeftPos; // Позиция "закрыто" (справа за экраном)
        private const float _openLeftPos = 0f;      // Позиция "открыто"
        private float _currentLeftPos;
        private bool _isInitialized = false;

        [SerializeField]
        private float _moveSpeed = 2500f; // Скорость перемещения в пикселях/сек

        private Coroutine _moveCoroutine;

        private void OnValidate()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: true);
        }

        private void Awake()
        {
            InitDocument();
        }

        private void Start()
        {
            MainSceneUIManager.Instance.Pages.Add(this.GetType(), this);
        }

        public void Show()
        {
            _document.rootVisualElement.SetDisplay(DisplayStyle.Flex);
        }

        public void Hide()
        {
            _document.rootVisualElement.SetDisplay(DisplayStyle.None);
        }

        private void InitDocument()
        {
            _document.enabled = true;
            _moveObject = QueryAndValidate<VisualElement>(_document.rootVisualElement, _moveObjectName, "Movement will not work.");
            _buttonOpenClose = QueryAndValidate<Button>(_document.rootVisualElement, _buttonOpenCloseName, "Button will not work.");
            _rootWindow = QueryAndValidate<VisualElement>(_document.rootVisualElement, _rootWindowID, "Movement will not be correctly initialized.");
            //_closeAreaElement = QueryAndValidate<VisualElement>(_document.rootVisualElement, _closeAreaElementID, "Close area will not work.");

            if (_moveObject == null || _buttonOpenClose == null || _rootWindow == null)
                return; // Прерываем, если хотя бы один важный элемент не найден

            // Подписываемся на изменение геометрии _rootWindow, чтобы узнать его ширину
            _rootWindow.RegisterCallback<GeometryChangedEvent>(OnRootWindowGeometryChanged);
            _buttonOpenClose.clicked += ClickedButtonOpenClose;

            //_closeAreaElement.RegisterCallback<PointerUpEvent>(Close);
            //_closeAreaElement.SetDisplay(DisplayStyle.None);
        }

        private void OnDestroy()
        {
            _rootWindow.UnregisterCallback<GeometryChangedEvent>(OnRootWindowGeometryChanged);

        }

        private T QueryAndValidate<T>(VisualElement container, string name, string errorMessage) where T : VisualElement
        {
            var element = container.Q<T>(name);
            if (element == null)
            {
                Debug.LogError($"'{name}' not found. {errorMessage}");
            }
            return element;
        }

        private void OnRootWindowGeometryChanged(GeometryChangedEvent evt)
        {
            // Если панель еще не инициализирована полностью, только обновляем _closedLeftPos
            if (!_isInitialized)
            {
                _closedLeftPos = _rootWindow.resolvedStyle.width;
                // Если это первый вызов и ширина уже есть, инициализируем
                if (_rootWindow.resolvedStyle.width != 0)
                {
                    _currentLeftPos = _closedLeftPos;
                    _moveObject.style.left = _currentLeftPos;
                    _isInitialized = true;
                }
                return;
            }

            // Если размер окна изменился, пересчитываем _closedLeftPos
            float oldClosedLeftPos = _closedLeftPos;
            _closedLeftPos = _rootWindow.resolvedStyle.width;

            // Если панель была закрыта, корректируем ее положение немедленно
            if (Mathf.Approximately(_currentLeftPos, oldClosedLeftPos))
            {
                _currentLeftPos = _closedLeftPos;
                _moveObject.style.left = _currentLeftPos;
            }
            // Если панель открыта, ее положение 0, ничего не меняем
            // Если панель в процессе анимации, анимация продолжится до _closedLeftPos или _openLeftPos
        }

        private void ClickedButtonOpenClose()
        {
            if (!_isInitialized)
                return;

            // Определяем целевую позицию: если панель закрыта - открываем, если открыта - закрываем
            float targetPos = (Mathf.Approximately(_currentLeftPos, _closedLeftPos)) ? _openLeftPos : _closedLeftPos;

            if (_moveCoroutine != null)
                StopCoroutine(_moveCoroutine);

            _moveCoroutine = StartCoroutine(MoveLeftPosition(targetPos));
        }

        private IEnumerator MoveLeftPosition(float target)
        {
            float start = _currentLeftPos;
            float distance = Mathf.Abs(target - start);

            if (distance < 0.01f)
                yield break;

            float direction = Mathf.Sign(target - start);
            float moved = 0f;

            while (moved < distance)
            {
                float delta = _moveSpeed * Time.deltaTime;
                moved += delta;

                if (moved > distance)
                    moved = distance;

                _currentLeftPos = start + direction * moved;
                _moveObject.style.left = _currentLeftPos;

                yield return null;
            }

            _currentLeftPos = target;
            _moveObject.style.left = _currentLeftPos;
        }
    }
}
