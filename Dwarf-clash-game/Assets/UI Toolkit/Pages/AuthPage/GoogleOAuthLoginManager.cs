using System;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GoogleOAuthLoginManager : BaseServiceBootstrapper
{
    [SerializeField]
    private ClientDataObject googleClientData;

    [SerializeField]
    private ClientDataObject googleAndroidClientData;

    [SerializeField]
    private ClientDataObject googleClientDataForEditor;

#if UNITY_WEBGL
    private OpenIDConnectService_WebGL _oidc;
#else
    private OpenIDConnectService _oidc;
#endif
    private AuthPage _authPage;

    protected override void Start()
    {
        base.Start();
        _authPage = GetComponent<AuthPage>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void RegisterServices()
    {
#if UNITY_WEBGL
        _oidc = new OpenIDConnectService_WebGL();
#else
        _oidc = new OpenIDConnectService();
#endif
        _oidc.OidcProvider = new GoogleOidcProvider();
#if UNITY_EDITOR
        _oidc.OidcProvider.ClientData = googleClientDataForEditor.clientData;
        _oidc.ServerListener.ListeningUri = _oidc.RedirectURI = "http://localhost:" + EditorUserBuildSettings.managedDebuggerFixedPort.ToString() + "/";
#elif UNITY_ANDROID
        _oidc.OidcProvider.ClientData = googleAndroidClientData.clientData;
        _oidc.RedirectURI = Application.absoluteURL;
#elif UNITY_WEBGL
        _oidc.OidcProvider.ClientData = googleClientData.clientData;
        _oidc.RedirectURI = "https://runewalk-dwarfclash-dev.ipshmykov.ru/"; // ToDo: заменить sed-ом в Gitlab CI на настройку в конфигурации
#else
        throw new PlatformNotSupportedException("This platform is not supported.");
#endif
        ServiceManager.RegisterService(_oidc);
        _oidc.LoginCompleted += OnLoginCompleted;

        _oidc.InitializeAsync();
    }

    protected override void UnRegisterServices()
    {
        _oidc.LoginCompleted -= OnLoginCompleted;
    }

    public void SignIn()
    {
        _oidc.OpenLoginPageAsync();
    }

    private void OnLoginCompleted(object sender, EventArgs e)
    {
        if (_oidc.IsLoggedIn)
        {
            _authPage.StartSignInTransition();
        }
        else
        {
            Debug.LogError("Sign In Failed");
        }
    }
}
