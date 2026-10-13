using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FMOD;
using FMODUnity;
using FMOD.Studio;
using Core.Utils;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuPage : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _buttonsRootElement;
        private Button _playButton;
        private Button _settingsButton;
        private const string _buttonsAreaID = "buttons_area";
        private const string _playButtonID = "b_play";
        private const string _settingsButtonID = "b_settings";
        private EVENT_CALLBACK _transitionCallback;

        [SerializeField]
        private SettingPage _settingsPage;

        [Header("Sound")]
        [SerializeField]
        private EventReference _mainMenuTrans_Event;
        private EventInstance _mainMenuTrans_Inst;
        [SerializeField]
        private EventReference _woodHover_Event;
        private EventInstance _woodHover_Inst;
        [SerializeField]
        private EventReference _woodTap_Event;
        private EventInstance _woodTap_Inst;

#if UNITY_EDITOR
        [SerializeField]
        private SceneAsset _scene;
#endif
        [SerializeField]
        [ReadOnly]
        private string _sceneName;

        private void OnValidate()
        {
#if UNITY_EDITOR
            _sceneName = _scene != null ? _scene.name : string.Empty;
#endif
            _document = GetComponentInChildren<UIDocument>(includeInactive: true);
        }

        private void Awake()
        {
            _buttonsRootElement = _document.rootVisualElement.Q(_buttonsAreaID);
            _playButton = _buttonsRootElement.Q<Button>(_playButtonID);
            _settingsButton = _buttonsRootElement.Q<Button>(_settingsButtonID);

            _transitionCallback = new EVENT_CALLBACK(TransitionFinished);

            RegisterButtonCallbacks();
        }

        private void OnDestroy()
        {
            UnregisterButtonCallbacks();
        }

        private void RegisterButtonCallbacks()
        {
            if (_playButton != null)
            {
                _playButton.RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
                _playButton.clicked += OnPlayButtonClicked;
            }
            if (_settingsButton != null)
            {
                _settingsButton.RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
                _settingsButton.clicked += OnSettingsButtonClicked;
            }
        }

        private void UnregisterButtonCallbacks()
        {
            if (_playButton != null)
            {
                _playButton.UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
                _playButton.clicked -= OnPlayButtonClicked;
            }
            if (_settingsButton != null)
            {
                _settingsButton.UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
                _settingsButton.clicked -= OnSettingsButtonClicked;
            }
        }

        private void OnPlayButtonClicked()
        {
            _buttonsRootElement.SetEnabled(false);
            PlayClickSound();

            _mainMenuTrans_Inst = RuntimeManager.CreateInstance(_mainMenuTrans_Event);
            _mainMenuTrans_Inst.setCallback(_transitionCallback);
            _mainMenuTrans_Inst.start();
            _mainMenuTrans_Inst.release();
        }

        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        private RESULT TransitionFinished(EVENT_CALLBACK_TYPE type, System.IntPtr instancePtr, System.IntPtr parameterPtr)
        {
            if (type == EVENT_CALLBACK_TYPE.STOPPED)
                FMODMainThreadDispatcher.Enqueue(() => SceneManager.LoadScene(_sceneName));

            return RESULT.OK;
        }

        private void OnSettingsButtonClicked()
        {
            PlayClickSound();
            _settingsPage.OpenSetting();
        }

        private void OnPointerEnterButton(PointerEnterEvent evt)
        {
            if (_buttonsRootElement.enabledSelf)
                PlayHoverSound();
        }

        private void PlayHoverSound()
        {
            _woodHover_Inst = RuntimeManager.CreateInstance(_woodHover_Event);
            _woodHover_Inst.start();
            _woodHover_Inst.release();
        }

        private void PlayClickSound()
        {
            _woodTap_Inst = RuntimeManager.CreateInstance(_woodTap_Event);
            _woodTap_Inst.start();
            _woodTap_Inst.release();
        }
    }
}
