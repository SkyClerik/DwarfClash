using UnityEditor;

[InitializeOnLoad]
public class EditorWebPortOverride
{

    /// <summary>
    /// Fixed port to override the autoselection mechanism. We need this to set fixed settings of listening URI for OAuth 2.0 workflows. 
    /// </summary>
    public const int EditorWebPort = 12346;
    
    static EditorWebPortOverride()
    {
        EditorUserBuildSettings.managedDebuggerFixedPort = EditorWebPort;
    }    
}
