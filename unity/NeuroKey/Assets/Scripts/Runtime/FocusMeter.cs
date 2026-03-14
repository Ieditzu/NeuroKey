using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Mathf;
using Gtec.UnityInterface; // EEGDataPipeline

/// <summary>
/// Lightweight focus indicator: computes a beta/alpha ratio on one EEG channel
/// and renders it as a percentage on screen. Intended for quick UX feedback
/// (not a clinical metric).
/// </summary>
public class FocusMeter : MonoBehaviour
{
    [Header("Data source")]
    [SerializeField] private EEGDataPipeline pipeline;
    [SerializeField] private int channelIndex = 0;          // 0 = first channel
    [SerializeField] private float sampleRateHz = 250f;     // override if your device differs
    [SerializeField] private int windowSamples = 256;       // ~1 s at 250 Hz

    [Header("UI (optional)")]
    [SerializeField] private TMP_Text focusText;

    private readonly List<float> _buffer = new List<float>(1024);

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

    private void Start()
    {
        EnsureLabel();
        UpdateLabel("--", "--");
    }

    private void EnsureLabel()
    {
        if (focusText != null)
            return;

        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return;

        var go = new GameObject("FocusLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, 0.84f);
        rt.anchorMax = new Vector2(0.35f, 0.92f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = new Color(0.2f, 0.95f, 1f);
        tmp.text = "Focus: --";
        focusText = tmp;
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

        UpdateLabel(focus01.ToString("P0"), ratio.ToString("0.00"));
    }

    private void UpdateLabel(string focusPercent, string ratio)
    {
        if (focusText == null)
            return;

        focusText.text = $"Focus: {focusPercent}\nβ/α ratio: {ratio}";
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
