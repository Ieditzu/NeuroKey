using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using Gtec.UnityInterface; // EEGDataPipeline

/// <summary>
/// Lightweight focus indicator: computes a beta/alpha ratio on one EEG channel
/// and renders it as a percentage on screen. Intended for quick UX feedback
/// (not a clinical metric).
/// </summary>
public class FocusMeter : MonoBehaviour
{
    private const string BciHudPrefKey = "BciHudEnabled";
    public static FocusMeter Instance { get; private set; }
    private static float? s_lastSimulatedAverageFocus01;
    private static bool s_hudEnabled = true;
    private static bool s_hudInitialized;

    [Header("Data source")]
    [SerializeField] private EEGDataPipeline pipeline;
    [SerializeField] private int channelIndex = 0;          // 0 = first channel
    [SerializeField] private float sampleRateHz = 250f;     // override if your device differs
    [SerializeField] private int windowSamples = 256;       // ~1 s at 250 Hz

    [Header("UI (optional)")]
    [SerializeField] private TMP_Text focusText;

    private Canvas _focusCanvas;

    private readonly List<float> _buffer = new List<float>(1024);
    private float _latestFocus01;
    private float _focusSum;
    private int _focusSampleCount;
    private float? _simulatedAverageFocus01;

    public bool HasSessionAverage => _focusSampleCount > 0;
    public float LatestFocus01 => _latestFocus01;
    public float AverageFocus01 => _focusSampleCount > 0 ? _focusSum / _focusSampleCount : 0f;
    public bool HasPipeline => pipeline != null;

    private void Awake()
    {
        Instance = this;
        EnsureHudInitialized();
    }

    private void OnEnable()
    {
        if (pipeline == null)
            pipeline = GetComponent<EEGDataPipeline>();

        if (pipeline == null)
            pipeline = FindObjectOfType<EEGDataPipeline>();

        if (pipeline != null)
            pipeline.OnEEGDataAvailable.AddListener(OnDataAvailable);
    }

    private void OnDisable()
    {
        if (pipeline != null)
            pipeline.OnEEGDataAvailable.RemoveListener(OnDataAvailable);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        EnsureLabel();
        UpdateHudVisibility();
        UpdateLabel("--", "--");
    }

    private void EnsureLabel()
    {
        if (!IsHudEnabled)
            return;

        if (focusText != null)
            return;

        // Create a dedicated overlay canvas so the label is always visible.
        var canvasGO = new GameObject("FocusCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = canvasGO.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.overrideSorting = true;
        c.sortingOrder = 999; // above other UI
        canvasGO.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        var targetCanvas = c;
        _focusCanvas = c;

        var go = new GameObject("FocusLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(targetCanvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.70f, 0.88f);
        rt.anchorMax = new Vector2(0.98f, 0.96f);
        rt.offsetMin = new Vector2(-12f, -8f);
        rt.offsetMax = new Vector2(-12f, -8f);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.raycastTarget = false;
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = new Color(0.18f, 0.95f, 1f);
        tmp.text = "Focus: --";
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.55f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
        focusText = tmp;

        Debug.Log("[FocusMeter] Created overlay label on canvas: " + targetCanvas.name);
    }

    private void OnDataAvailable(float[,] samples)
    {
        if (samples == null)
            return;

        int frames = samples.GetLength(0);
        int channels = samples.GetLength(1);
        if (channelIndex < 0 || channelIndex >= channels)
            return;

        for (int i = 0; i < frames; i++)
        {
            _buffer.Add(samples[i, channelIndex]);
        }

        // keep only the newest window
        int maxKeep = Max(windowSamples, 32);
        if (_buffer.Count > maxKeep)
            _buffer.RemoveRange(0, _buffer.Count - maxKeep);

        if (_buffer.Count < windowSamples)
            return; // not enough data yet

        double alphaPower = GoertzelPower(_buffer, 10.0); // center ~10 Hz
        double betaPower = GoertzelPower(_buffer, 20.0);  // center ~20 Hz

        double ratio = betaPower / (alphaPower + 1e-6);
        float focus01 = Clamp01((float)(ratio / 3.0)); // heuristic scaling
        _latestFocus01 = focus01;
        _focusSum += focus01;
        _focusSampleCount++;

        UpdateLabel(focus01.ToString("P0"), ratio.ToString("0.00"));
    }

    private void UpdateLabel(string focusPercent, string ratio)
    {
        if (!IsHudEnabled)
            return;

        if (focusText == null)
            return;

        focusText.text = $"Focus: {focusPercent}\nβ/α ratio: {ratio}";
    }

    public string GetAverageSummary(bool romanian)
    {
        if (!HasSessionAverage)
        {
            if (ShouldUseSimulatedAverage())
            {
                float simulated = GetSimulatedAverageFocus01();
                return romanian
                    ? $"Focus mediu: {simulated:P0} (simulat - fara flux EEG primit)"
                    : $"Average focus: {simulated:P0} (simulated - no EEG stream received)";
            }

            return romanian
                ? "Focus mediu: dispozitivul BCI nu este conectat."
                : "Average focus: BCI device is not connected.";
        }

        return romanian
            ? $"Focus mediu: {AverageFocus01:P0}"
            : $"Average focus: {AverageFocus01:P0}";
    }

    public static string GetAverageSummaryForCurrentScene(bool romanian)
    {
        if (Instance != null)
            return Instance.GetAverageSummary(romanian);

        if (UnicornCompatibility.IsAvailable)
        {
            float simulated = GetStaticSimulatedAverageFocus01();
            return romanian
                ? $"Focus mediu: {simulated:P0} (simulat - fara flux EEG primit)"
                : $"Average focus: {simulated:P0} (simulated - no EEG stream received)";
        }

        return romanian
            ? "Focus mediu: dispozitivul BCI nu este conectat."
            : "Average focus: BCI device is not connected.";
    }

    private bool ShouldUseSimulatedAverage()
    {
        return HasPipeline || UnicornCompatibility.IsAvailable;
    }

    private float GetSimulatedAverageFocus01()
    {
        if (_simulatedAverageFocus01.HasValue)
            return _simulatedAverageFocus01.Value;

        _simulatedAverageFocus01 = UnityEngine.Random.Range(0.58f, 0.87f);
        return _simulatedAverageFocus01.Value;
    }

    public static bool IsHudEnabled
    {
        get
        {
            EnsureHudInitialized();
            return s_hudEnabled;
        }
    }

    public static void SetHudEnabled(bool enabled, bool persist = true)
    {
        EnsureHudInitialized();
        s_hudEnabled = enabled;
        if (persist)
        {
            PlayerPrefs.SetInt(BciHudPrefKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        if (Instance != null)
            Instance.UpdateHudVisibility();
    }

    private static void EnsureHudInitialized()
    {
        if (s_hudInitialized)
            return;
        s_hudInitialized = true;
        s_hudEnabled = PlayerPrefs.GetInt(BciHudPrefKey, 1) == 1;
    }

    private void UpdateHudVisibility()
    {
        if (IsHudEnabled && focusText == null)
            EnsureLabel();

        if (_focusCanvas != null)
            _focusCanvas.gameObject.SetActive(IsHudEnabled);

        if (focusText != null)
            focusText.gameObject.SetActive(IsHudEnabled);
    }

    private static float GetStaticSimulatedAverageFocus01()
    {
        if (s_lastSimulatedAverageFocus01.HasValue)
            return s_lastSimulatedAverageFocus01.Value;

        s_lastSimulatedAverageFocus01 = UnityEngine.Random.Range(0.58f, 0.87f);
        return s_lastSimulatedAverageFocus01.Value;
    }

    private double GoertzelPower(IReadOnlyList<float> data, double targetHz)
    {
        int n = data.Count;
        if (n == 0 || sampleRateHz <= 0.0f)
            return 0;

        double k = 0.5 + ((n * targetHz) / sampleRateHz);
        double omega = (2.0 * Math.PI * k) / n;
        double sine = Math.Sin(omega);
        double cosine = Math.Cos(omega);
        double coeff = 2.0 * cosine;

        double q0 = 0, q1 = 0, q2 = 0;
        for (int i = 0; i < n; i++)
        {
            q0 = coeff * q1 - q2 + data[i];
            q2 = q1;
            q1 = q0;
        }

        double power = q1 * q1 + q2 * q2 - coeff * q1 * q2;
        return power / n;
    }
}
