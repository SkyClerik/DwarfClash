using UnityEngine;

namespace Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "ApplicationSettings", menuName = "Data/Application/ApplicationSettings")]
    public class ApplicationSettings : ScriptableObject
    {
        [SerializeField]
        private int _targetFPS;
        [SerializeField]
        private float _musicVolume;
        [SerializeField]
        private float _effectsVolume;

        public int TargetFPS { get => _targetFPS; set => _targetFPS = value; }
        public float MusicVolume { get => _musicVolume; set => _musicVolume = value; }
        public float EffectsVolume { get => _effectsVolume; set => _effectsVolume = value; }
    }
}