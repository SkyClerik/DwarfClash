using Data;
using FMOD.Studio;
using FMODUnity;
using Toolbox;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingPage : MonoBehaviour
    {
        [SerializeField]
        private ApplicationSettings _settings;
        [SerializeField]
        [ReadOnly]
        private UIDocument _document;
        private VisualElement _settingWindow;
        private VisualElement _graphicsWindow;

        private const string _mixerMusicChanelName = "Music";
        private const string _mixerMasterName = "Master";
        private const string _soundButtonName = "sound";
        private const string _graphicsButtonName = "graphics";

        private const string _discordButtonName = "discord";
        private const string _xButtonName = "x";
        private const string _vkButtonName = "vk";
        private const string _privacyButtonName = "privacy";
        private const string _tellUsButtonName = "tell_us";
        private const string _closeGraphicsButtonName = "close_graphics";
        private const string _closeSettingButtonName = "close_setting";

        private const string _musicSliderName = "music_slider";
        private const string _fpsToggleName = "framerate";
        private const string _graphicsRadioButtonGroupName = "radio_button_group_graphics";

        private const string _modalCoverName = "modal_cover";

        private const string _settingWindowName = "setting_window";
        private const string _graphicsWindowName = "graphics_window";

        //private EventCallback<ChangeEvent<bool>> _soundMuteCallback;
        //private EventCallback<ChangeEvent<bool>> _musicMuteCallback;
        private EventCallback<ChangeEvent<bool>> _setFpsCallback;
        private EventCallback<ChangeEvent<int>> _setGraphicsQualityCallback;
        private EventCallback<ChangeEvent<float>> _changeMusicVolumeCallback;
        private EventCallback<ClickEvent> _hideSettingWindowCallback;
        private EventCallback<ClickEvent> _hideGraphicsWindowCallback;

        [Header("Sound")]

        [SerializeField]
        private EventReference _woodHover_Event;
        private EventInstance _woodHover_Inst;
        [SerializeField]
        private EventReference _woodTap_Event;
        private EventInstance _woodTap_Inst;

        [SerializeField]
        private VCA _vCAController;
        private const string _vcaMusicName = "vca:/Music";

        private void OnValidate()
        {
            _document = GetComponentInChildren<UIDocument>(includeInactive: true);
        }

        void Start()
        {
            _document.enabled = true;
            _settingWindow = _document.rootVisualElement.Q<VisualElement>(_settingWindowName);
            _graphicsWindow = _document.rootVisualElement.Q<VisualElement>(_graphicsWindowName);
            Hide(_settingWindow.parent);
            Hide(_graphicsWindow.parent);

            _vCAController = RuntimeManager.GetVCA(_vcaMusicName);
            SetStartingValues();

#if DEBUG || UNITY_EDITOR
            // Выводим в консоль все загруженные банки для отладки
            Invoke(nameof(PrintLoadedBanksForDebug), 1f);
#endif

            SubscribeToButtons();
        }

        private void SetStartingValues()
        {
            _vCAController.setVolume(_settings.MusicVolume);
            _settingWindow.Q<Slider>(_musicSliderName).value = _settings.MusicVolume;
        }

        private void PrintLoadedBanksForDebug()
        {
            Debug.Log("--- [DEBUG] ПОИСК VCA ВО ВСЕХ ЗАГРУЖЕННЫХ БАНКАХ ---");
            var system = FMODUnity.RuntimeManager.StudioSystem;
            system.getBankList(out FMOD.Studio.Bank[] allBanks);

            if (allBanks == null)
            {
                Debug.LogWarning("[DEBUG] Не удалось получить список банков.");
                return;
            }

            bool vcaFound = false;
            foreach (var bank in allBanks)
            {
                bank.getVCACount(out int vcaCount);
                if (vcaCount > 0)
                {
                    bank.getVCAList(out FMOD.Studio.VCA[] vcas);
                    foreach (var vca in vcas)
                    {
                        vca.getPath(out string vcaPath);
                        if (!string.IsNullOrEmpty(vcaPath))
                        {
                            vcaFound = true;
                            Debug.Log($"[DEBUG] Найден VCA: '{vcaPath}'");
                        }
                    }
                }
            }

            if (!vcaFound)
            {
                Debug.LogWarning("[DEBUG] Ни одного VCA не найдено в загруженных банках.");
            }
            Debug.Log("--------------------------------------------------");
        }

        void OnDestroy()
        {
            UnsubscribeFromButtons();
        }

        public void OpenSetting()
        {
            Show(_settingWindow.parent);
        }

        private void SubscribeToButtons()
        {
            _settingWindow.Q<Button>(_graphicsButtonName).clicked += OnButtonClickedGraphics;
            _settingWindow.Q<Button>(_closeSettingButtonName).clicked += OnButtonClickedCloseSetting;
            _settingWindow.Q<Button>(_vkButtonName).clicked += OnButtonClickedVk;
            _settingWindow.Q<Button>(_discordButtonName).clicked += OnButtonClickedDiscord;
            _settingWindow.Q<Button>(_xButtonName).clicked += OnButtonClickedX;
            _settingWindow.Q<Button>(_privacyButtonName).clicked += OnButtonClickedPrivacy;
            _settingWindow.Q<Button>(_tellUsButtonName).clicked += OnButtonClickedSendUs;
            _settingWindow.Q<Button>(_soundButtonName).clicked += OnButtonClickedSound;

            _settingWindow.Q<Button>(_graphicsButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_closeSettingButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_vkButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_discordButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_xButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_privacyButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_tellUsButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_soundButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);

            _changeMusicVolumeCallback = volume => OnChangeMusicVolume(volume);
            _settingWindow.Q<Slider>(_musicSliderName).RegisterValueChangedCallback(_changeMusicVolumeCallback);

            _hideSettingWindowCallback = ev => Hide(_settingWindow.parent);
            _settingWindow.parent.Q<VisualElement>(_modalCoverName).RegisterCallback(_hideSettingWindowCallback);

            _graphicsWindow.Q<Button>(_closeGraphicsButtonName).clicked += OnButtonClickedCloseGraphics;
            _graphicsWindow.Q<Button>(_privacyButtonName).clicked += OnButtonClickedPrivacy;
            _graphicsWindow.Q<Button>(_tellUsButtonName).clicked += OnButtonClickedSendUs;

            _graphicsWindow.Q<Button>(_closeGraphicsButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _graphicsWindow.Q<Button>(_privacyButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _graphicsWindow.Q<Button>(_tellUsButtonName).RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);

            _setFpsCallback = ev => SetFps(ev);
            _setGraphicsQualityCallback = ev => SetGraphicsQuality(ev);
            _hideGraphicsWindowCallback = ev => Hide(_graphicsWindow.parent);
            _graphicsWindow.Q<Toggle>(_fpsToggleName).RegisterValueChangedCallback(_setFpsCallback);
            _graphicsWindow.Q<RadioButtonGroup>(_graphicsRadioButtonGroupName).RegisterValueChangedCallback(_setGraphicsQualityCallback);
            _graphicsWindow.parent.Q<VisualElement>(_modalCoverName).RegisterCallback(_hideGraphicsWindowCallback);
        }

        private void UnsubscribeFromButtons()
        {
            _settingWindow.Q<Button>(_graphicsButtonName).clicked -= OnButtonClickedGraphics;
            _settingWindow.Q<Button>(_closeSettingButtonName).clicked -= OnButtonClickedCloseSetting;
            _settingWindow.Q<Button>(_vkButtonName).clicked -= OnButtonClickedVk;
            _settingWindow.Q<Button>(_discordButtonName).clicked -= OnButtonClickedDiscord;
            _settingWindow.Q<Button>(_xButtonName).clicked -= OnButtonClickedX;
            _settingWindow.Q<Button>(_privacyButtonName).clicked -= OnButtonClickedPrivacy;
            _settingWindow.Q<Button>(_tellUsButtonName).clicked -= OnButtonClickedSendUs;
            _settingWindow.Q<Button>(_soundButtonName).clicked -= OnButtonClickedSound;
            _settingWindow.Q<Button>(_graphicsButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_closeSettingButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_vkButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_discordButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_xButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_privacyButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_tellUsButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _settingWindow.Q<Button>(_soundButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);

            _graphicsWindow.Q<Button>(_closeGraphicsButtonName).clicked -= OnButtonClickedCloseGraphics;
            _graphicsWindow.Q<Button>(_privacyButtonName).clicked -= OnButtonClickedPrivacy;
            _graphicsWindow.Q<Button>(_tellUsButtonName).clicked -= OnButtonClickedSendUs;
            _graphicsWindow.Q<Button>(_closeGraphicsButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _graphicsWindow.Q<Button>(_privacyButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _graphicsWindow.Q<Button>(_tellUsButtonName).UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);

            _changeMusicVolumeCallback = ToolkitExt.TryUnregisterValueChangedCallback(_settingWindow.Q<Slider>(_musicSliderName), _changeMusicVolumeCallback);
            _hideSettingWindowCallback = ToolkitExt.TryUnregisterCallback(_settingWindow.parent.Q<VisualElement>(_modalCoverName), _hideSettingWindowCallback);
            _setFpsCallback = ToolkitExt.TryUnregisterValueChangedCallback(_graphicsWindow.Q<Toggle>(_fpsToggleName), _setFpsCallback);
            _setGraphicsQualityCallback = ToolkitExt.TryUnregisterValueChangedCallback(_graphicsWindow.Q<RadioButtonGroup>(_graphicsRadioButtonGroupName), _setGraphicsQualityCallback);
            _hideGraphicsWindowCallback = ToolkitExt.TryUnregisterCallback(_graphicsWindow.parent.Q<VisualElement>(_modalCoverName), _hideGraphicsWindowCallback);
        }

        private void SetFps(ChangeEvent<bool> ev)
        {
            if (ev.newValue) Application.targetFrameRate = 60;
            else Application.targetFrameRate = 30;
        }

        private void SetGraphicsQuality(ChangeEvent<int> ev)
        {
            switch (ev.newValue)
            {
                case 0:
                    Debug.Log("low graphics set");
                    break;
                case 1:
                    Debug.Log("med graphics set");
                    break;
                case 2:
                    Debug.Log("high graphics set");
                    break;
                default:
                    break;
            }
        }

        private void Show(VisualElement overlay)
        {
            overlay.style.display = DisplayStyle.Flex;
        }

        private void Hide(VisualElement overlay)
        {
            overlay.style.display = DisplayStyle.None;
        }

        private void OnButtonClickedGraphics()
        {
            PlayClickSound();
            Show(_graphicsWindow.parent);
        }

        private void OnButtonClickedPrivacy()
        {
            PlayClickSound();
            Debug.Log("privacy_pressed");
        }

        private void OnButtonClickedSendUs()
        {
            PlayClickSound();
            Debug.Log("send_us_pressed");
        }

        private void OnButtonClickedDiscord()
        {
            PlayClickSound();
            Debug.Log("discord_pressed");
        }

        private void OnButtonClickedX()
        {
            PlayClickSound();
            Debug.Log("x_pressed");
        }

        private void OnButtonClickedVk()
        {
            PlayClickSound();
            Debug.Log("vk_pressed");
        }

        private void OnButtonClickedSound()
        {
            PlayClickSound();
            Debug.Log("sound_pressed");
        }

        private void OnButtonClickedCloseGraphics()
        {
            PlayClickSound();
            Hide(_graphicsWindow.parent);
        }

        private void OnButtonClickedCloseSetting()
        {
            PlayClickSound();
            Hide(_settingWindow.parent);
        }

        private void OnChangeMusicVolume(ChangeEvent<float> ev)
        {
            _settings.MusicVolume = ev.newValue;
            _vCAController.setVolume(ev.newValue);
        }

        private void OnPointerEnterButton(PointerEnterEvent evt)
        {
            //if (_buttonsArea.enabledSelf)
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
