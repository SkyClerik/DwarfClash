using UnityEngine;
using UnityEngine.UIElements;
using Toolbox;

[RequireComponent(typeof(UIDocument))]
public class BattleTimePage : Singleton<BattleTimePage>
{
    private UIDocument _document;
    private VisualElement root;
    private Button x1Button;
    private Button x2Button;
    private Button x4Button;

    private float timeScale = 1f;

    private void Start()
    {
        _document = GetComponent<UIDocument>();
        root = _document.rootVisualElement;

        // Создание кнопок
        x1Button = new Button { text = "x1" };
        x2Button = new Button { text = "x2" };
        x4Button = new Button { text = "x4" };

        // Добавление кнопок в правый верхний угол экрана
        var container = new VisualElement { style = { position = Position.Absolute, top = 0, right = 0 } };
        container.Add(x1Button);
        container.Add(x2Button);
        container.Add(x4Button);
        root.Add(container);

        // Регистрация событий нажатия кнопок
        x1Button.clicked += () => SetTimeScale(1f);
        x2Button.clicked += () => SetTimeScale(2f);
        x4Button.clicked += () => SetTimeScale(4f);
    }

    private void SetTimeScale(float scale)
    {
        timeScale = scale;
        Time.timeScale = timeScale;
    }
}
