using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay
{
    public class Billboard : VisualElement
    {
        private GameObject _sceneObject;
        private double _time;
        private const int _oneSecond = 1;
        private float _iteration = 1;

        private VisualElement _image;
        private Label _timer;
        private const string _imageName = "image";
        private const string _labelName = "label";
        private const string _timeSpanSettings = @"\:mm\:ss";

        public GameObject GetSceneObject => _sceneObject;
        public Vector2 CanvasPosition;

        private Action _onTimeUp;

        public virtual void Init(VisualTreeAsset template, VisualElement root)
        {
            template.CloneTree(this);
            root.Add(this);
            style.position = Position.Absolute;

            _image = this.Q(_imageName);
            _timer = this.Q<Label>(_labelName);
        }

        public void SetImage(Sprite sprite)
        {
            _image.style.backgroundImage = new StyleBackground(sprite);
        }

        public void Reset(GameObject sceneObject, TimeSpan timeout, Action onTimeUp)
        {
            _sceneObject = sceneObject;
            _onTimeUp = onTimeUp;
            _time = timeout.TotalSeconds;
            _iteration = 1;
            Show();
        }

        public void Tick()
        {
            if (_time <= 0)
            {
                Hide();
                _sceneObject = null;

                _onTimeUp?.Invoke();
                _onTimeUp = null;

                return;
            }

            _timer.text = TimeSpan.FromSeconds(_time).ToString(_timeSpanSettings);

            _iteration -= Time.fixedDeltaTime;
            if (_iteration <= 0)
            {
                _iteration = 1;
                _time -= _oneSecond;
            }
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
        }

        public void Show()
        {
            style.display = DisplayStyle.Flex;
        }
    }
}