using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Profiling;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(UIDocument))]
public class SimpleStatsDisplay : MonoBehaviour
{
    // --- From Graphy ---
    private static readonly Color ColorGood = new Color(0.208f, 0.679f, 0.621f);
    private static readonly Color ColorCaution = new Color(0.913f, 0.768f, 0.415f);
    private static readonly Color ColorCritical = new Color(0.905f, 0.435f, 0.317f);
    private static readonly Color ColorRam = new Color(0.945f, 0.356f, 0.709f);
    private static readonly Color ColorBackground = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    private static readonly Color ColorText = Color.white;
    private static readonly Color ColorAdvancedText = new Color(0.8f, 0.8f, 0.8f);

    [Header("Settings")]
    [SerializeField] private int _fpsGoodThreshold = 60;
    [SerializeField] private int _fpsCautionThreshold = 30;
    [SerializeField] [Range(1, 60)] private int _fpsSamples = 30;

    private Label _fpsLabel;
    private Label _ramLabel;
    private Label _advancedLabel;

    private Queue<float> _fpsBuffer;

    void OnEnable()
    {
        // Unlock framerate for performance measurement
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        _fpsBuffer = new Queue<float>(_fpsSamples);
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // Create container
        var container = new VisualElement();
        container.style.position = Position.Absolute;
        container.style.left = 10;
        container.style.top = 10;
        container.style.paddingTop = 5;
        container.style.paddingBottom = 5;
        container.style.paddingLeft = 10;
        container.style.paddingRight = 10;
        container.style.backgroundColor = ColorBackground;
        container.style.borderTopLeftRadius = 5;
        container.style.borderTopRightRadius = 5;
        container.style.borderBottomLeftRadius = 5;
        container.style.borderBottomRightRadius = 5;
        
        // Create labels
        _fpsLabel = CreateStatLabel(container, "FPS");
        _ramLabel = CreateStatLabel(container, "RAM");
        _advancedLabel = CreateStatLabel(container, "SYS");
        _advancedLabel.style.marginTop = 5;
        _advancedLabel.style.unityFontStyleAndWeight = FontStyle.Italic;

        root.Add(container);
        
        // Initial data population
        _advancedLabel.text = $"SYS: {SystemInfo.processorType}\nGPU: {SystemInfo.graphicsDeviceName}\nSCR: {Screen.width}x{Screen.height}";
    }

    void OnDisable()
    {
        // Clean up the UI when the component is disabled or destroyed
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root != null && root.childCount > 0)
        {
            root.Clear();
        }
    }

    private Label CreateStatLabel(VisualElement parent, string prefix)
    {
        var label = new Label { text = prefix + ": ..." };
        label.style.color = ColorText;
        label.style.fontSize = 14;
        parent.Add(label);
        return label;
    }

    void Update()
    {
        // -- FPS --
        float currentFps = 1f / Time.unscaledDeltaTime;
        _fpsBuffer.Enqueue(currentFps);
        if (_fpsBuffer.Count > _fpsSamples)
        {
            _fpsBuffer.Dequeue();
        }
        float averageFps = _fpsBuffer.Average();
        
        _fpsLabel.text = $"FPS: {averageFps:F0}";
        if (averageFps >= _fpsGoodThreshold)
            _fpsLabel.style.color = ColorGood;
        else if (averageFps >= _fpsCautionThreshold)
            _fpsLabel.style.color = ColorCaution;
        else
            _fpsLabel.style.color = ColorCritical;

        // -- RAM --
        long allocatedBytes = Profiler.GetTotalAllocatedMemoryLong();
        long reservedBytes = Profiler.GetTotalReservedMemoryLong();
        _ramLabel.text = $"RAM: {allocatedBytes / 1048576f:F1}MB / {reservedBytes / 1048576f:F1}MB";
        _ramLabel.style.color = ColorRam;
    }
}
