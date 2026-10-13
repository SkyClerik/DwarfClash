using Core.Utils;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument), typeof(GoogleOAuthLoginManager))]
public class AuthPage : MonoBehaviour // Revert to MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _buttonsRootElement;
    private Button _loginButton;
    private const string _buttonsAreaID = "buttons_area";
    private const string _loginButtonID = "b_login";
    private GoogleOAuthLoginManager _loginManager;

    private EVENT_CALLBACK _transitionCallback;

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

    private void Awake() // Revert to private void
    {
        _document = GetComponent<UIDocument>();
        _loginManager = GetComponent<GoogleOAuthLoginManager>();

        var root = _document.rootVisualElement;
        _buttonsRootElement = root.Q(_buttonsAreaID);
        _loginButton = root.Q<Button>(_loginButtonID);

        _transitionCallback = new EVENT_CALLBACK(TransitionFinished);

        RegisterButtonCallbacks();
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private RESULT TransitionFinished(EVENT_CALLBACK_TYPE type, System.IntPtr instancePtr, System.IntPtr parameterPtr) // Revert to non-static
    {
        if (type == EVENT_CALLBACK_TYPE.STOPPED)
            FMODMainThreadDispatcher.Enqueue(() => SceneManager.LoadScene(_sceneName)); // Access _sceneName directly

        return RESULT.OK;
    }

    private void OnDestroy()
    {
        UnregisterButtonCallbacks();
    }

    private void RegisterButtonCallbacks()
    {
        if (_loginButton != null)
        {
            _loginButton.RegisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _loginButton.clicked += OnLoginButtonClicked;
        }
    }

    private void UnregisterButtonCallbacks()
    {
        if (_loginButton != null)
        {
            _loginButton.UnregisterCallback<PointerEnterEvent>(OnPointerEnterButton);
            _loginButton.clicked -= OnLoginButtonClicked;
        }
    }

    private void OnLoginButtonClicked()
    {
        _buttonsRootElement.SetEnabled(false);
        PlayClickSound();
        _loginManager.SignIn();
    }

    public void StartSignInTransition()
    {
        _mainMenuTrans_Inst = RuntimeManager.CreateInstance(_mainMenuTrans_Event);
        _mainMenuTrans_Inst.setCallback(_transitionCallback);
        _mainMenuTrans_Inst.start();
        _mainMenuTrans_Inst.release();
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
