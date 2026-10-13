using Data;
using System;
using System.Collections;
using Toolbox;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class BuildingsPage : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField]
        private UIDocument _document;
        [SerializeField]
        private bool _useIntermediateProgress = true;
        [SerializeField]
        private int _mainProgressSteps = 10;

        [Header("Plugins")]

        [SerializeField]
        private BuildingDefinition _buildingDefinition;

        [Header("Debug")]

        [SerializeField, ReadOnly]
        private string _displayedTotalDurationString;
        [SerializeField, ReadOnly]
        private string _remainingTimeString;

        //---

        private VisualElement _root;
        private Label _waitngTime;
        private const string _waitngTimeID = "waitng_time";
        private VisualElement _progressBar;
        private VisualElement _secondProgress;
        private VisualElement _mainProgress;
        private const string _progressBarID = "progress_bar";
        private const string _secondProgressID = "second_progress";
        private const string _mainProgressID = "main_progress";
        private Coroutine _progressCoroutine;

        private float _cachedMainProgressValue;
        private float _cachedSecondProgressValue;
        private float _cachedOverallProgress;
        private int _cachedCurrentStep;
        private float _cachedStepPercent;
        private int _cachedPreviousStep;
        private double _cachedSecondsForNextStep;
        private DateTime _cachedStartTime;
        private DateTime _cachedEndTime;
        private TimeSpan _cachedTotalDuration;
        private TimeSpan _cachedElapsed;
        private TimeSpan _cachedTimeToCompletion;
        private DateTime _cachedNextStepTime;
        private TimeSpan _cachedTimeToNextStep;

        public UIDocument GetUIDocument => _document;

        private void OnValidate()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: true);
        }

        private void Start()
        {
            MainSceneUIManager.Instance.Pages.Add(this.GetType(), this);
            Init();
        }

        private void Init()
        {
            _document.enabled = true;
            _root = _document.rootVisualElement;

            _waitngTime = _root.Q<Label>(_waitngTimeID);

            _progressBar = _root.Q(_progressBarID);
            _secondProgress = _progressBar.Q(_secondProgressID);
            _mainProgress = _progressBar.Q(_mainProgressID);

            _mainProgress.SetWidthPercentage(0);
            _secondProgress.SetWidthPercentage(0);

            _secondProgress.style.display = _useIntermediateProgress ? DisplayStyle.Flex : DisplayStyle.None;

            _cachedStepPercent = 100f / _mainProgressSteps;
            _cachedStartTime = _buildingDefinition.StartTime.Value;
            _cachedEndTime = _cachedStartTime + _buildingDefinition.TotalDuration.Value;
            _cachedTotalDuration = _cachedEndTime - _cachedStartTime;
            _buildingDefinition.EndTime.Value = _cachedEndTime;
            _displayedTotalDurationString = _cachedTotalDuration.ToString();

            Hide();
        }

        private void StartProgress()
        {
            if (_progressCoroutine == null)
            {
                _cachedPreviousStep = -1;
                _cachedSecondsForNextStep = 0;
                _progressCoroutine = StartCoroutine(ProgressCoroutine());
            }
        }

        private void StopProgress()
        {
            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            StopProgress();
        }

        private IEnumerator ProgressCoroutine()
        {
            if (_cachedTotalDuration.TotalSeconds <= 0)
                yield break;

            while (DateTime.Now < _cachedEndTime)
            {
                _cachedElapsed = DateTime.Now - _cachedStartTime;
                _cachedOverallProgress = (float)(_cachedElapsed.TotalSeconds / _cachedTotalDuration.TotalSeconds) * 100f;

                _cachedTimeToCompletion = _cachedEndTime - DateTime.Now;
                if (_cachedTimeToCompletion.TotalSeconds < 0)
                    _cachedTimeToCompletion = TimeSpan.Zero;

                if (_useIntermediateProgress)
                {
                    _cachedCurrentStep = (int)(_cachedOverallProgress / _cachedStepPercent);

                    if (_cachedCurrentStep != _cachedPreviousStep)
                    {
                        _cachedMainProgressValue = _cachedCurrentStep * _cachedStepPercent;
                        _mainProgress.SetWidthPercentage(_cachedMainProgressValue);

                        _cachedSecondProgressValue = (_cachedCurrentStep + 1) * _cachedStepPercent;
                        _secondProgress.SetWidthPercentage(_cachedSecondProgressValue);

                        _cachedSecondsForNextStep = _cachedTotalDuration.TotalSeconds * (_cachedSecondProgressValue / 100.0);

                        _cachedPreviousStep = _cachedCurrentStep;
                    }

                    _cachedNextStepTime = _cachedStartTime.AddSeconds(_cachedSecondsForNextStep);
                    _cachedTimeToNextStep = _cachedNextStepTime - DateTime.Now;

                    if (_cachedTimeToNextStep.TotalSeconds < 0)
                        _cachedTimeToNextStep = TimeSpan.Zero;

                    _waitngTime.text = _cachedTimeToNextStep.ToReadableString();
                }
                else
                {
                    _mainProgress.SetWidthPercentage(_cachedOverallProgress);
                    _waitngTime.text = _cachedTimeToCompletion.ToReadableString();
                }

                _remainingTimeString = _cachedTimeToCompletion.ToReadableString();

                yield return new WaitForSeconds(1f);
            }

            // Finalize progress
            _mainProgress.SetWidthPercentage(100);
            _secondProgress.SetWidthPercentage(100);
            _waitngTime.text = "Полный бонус";
            StopProgress();
            _remainingTimeString = TimeSpan.Zero.ToReadableString();
        }

        public void Show()
        {
            StartProgress();
            _root.SetDisplay(DisplayStyle.Flex);
        }

        public void Hide()
        {
            StopProgress();
            _root.SetDisplay(DisplayStyle.None);
        }
    }
}
