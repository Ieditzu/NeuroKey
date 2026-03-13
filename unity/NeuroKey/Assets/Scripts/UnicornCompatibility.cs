using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Lightweight runtime probe so the app can tell whether the Unicorn Hybrid Black
/// libraries are present on the current platform before trying to use them.
/// This avoids hard references to the vendor assemblies (which would break builds
/// on platforms where the plugins are excluded).
/// </summary>
public class UnicornCompatibility : MonoBehaviour
{
    private static readonly string[] AssemblyNames =
    {
        "UnicornDotNet",
        "Gtec.UnityInterface"
    };

    private static readonly string[] NativeDlls =
    {
        "Unicorn.dll",
        "Gtec.Chain.Windows.Devices.Unicorn.dll",
        "Gtec.Chain.Mac.Devices.Unicorn.dll",
        "Gtec.Chain.Android.Unity.Devices.Unicorn.dll"
    };

    /// <summary>
    /// True when both the managed and native Unicorn bits can be found.
    /// </summary>
    public static bool IsAvailable { get; private set; }

    /// <summary>
    /// If false, a short reason is stored here for UI/logging.
    /// </summary>
    public static string AvailabilityReason { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Detect()
    {
        // Managed assemblies present?
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var managedOk = AssemblyNames.All(loaded.Contains);
        if (!managedOk)
        {
            AvailabilityReason = "Managed Unicorn assemblies are not loaded (expected UnicornDotNet + Gtec.UnityInterface).";
            IsAvailable = false;
            return;
        }

        // Native plugins present on disk (Editor/Windows path under Assets/Plugins).
        var pluginsRoot = Path.Combine(Application.dataPath, "g.tec", "Unity Interface", "Plugins");
        var nativesOk = NativeDlls.Any(dll => File.Exists(Path.Combine(pluginsRoot, dll)));
        if (!nativesOk)
        {
            AvailabilityReason = $"No Unicorn native DLLs found under {pluginsRoot}.";
            IsAvailable = false;
            return;
        }

        IsAvailable = true;
        AvailabilityReason = "Unicorn Hybrid Black runtime detected.";
        Debug.Log("[Unicorn] Compatibility OK: managed + native plugins located.");
    }

    /// <summary>
    /// Helper for UI to display a friendly message.
    /// </summary>
    public static string GetStatusLabel()
    {
        return IsAvailable ? "Unicorn ready" : $"Unicorn unavailable: {AvailabilityReason}";
    }

    private void OnValidate()
    {
        // Keep developer feedback visible in the Inspector.
        if (!Application.isPlaying)
        {
            Debug.Log(GetStatusLabel());
        }
    }
}
