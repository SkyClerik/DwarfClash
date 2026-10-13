using UnityEngine;
using Toolbox;
using FMODUnity;
using FMOD.Studio;

public class GlobalBackgroundSound : Singleton<GlobalBackgroundSound>
{
    [SerializeField]
    private EventReference _mainMenuMus_Event;
    private EventInstance _mainMenuMus_Inst;

    private void OnEnable()
    {
        GlobalBackgroundSound.Instance.StartBackgroundSound();
        DontDestroyOnLoad(this);
    }

    private void StartBackgroundSound()
    {
        _mainMenuMus_Inst = RuntimeManager.CreateInstance(_mainMenuMus_Event);
        _mainMenuMus_Inst.start();
    }

    public void StopBackgroundSound()
    {
        if (_mainMenuMus_Inst.isValid())
        {
            _mainMenuMus_Inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _mainMenuMus_Inst.release();
        }
    }
}
