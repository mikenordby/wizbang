using UnityEngine;

/// <summary>
/// Unified debug logging system that outputs to BOTH Unity Console AND persistent log files.
/// Uses [Conditional] attribute to remove calls entirely in Release builds.
///
/// UNIFIED ARCHITECTURE:
/// - All logs go to Unity Console (for immediate feedback)
/// - INFO/WARN/ERROR also go to log files (for Claude Code debugging)
/// - VERBOSE stays console-only (to avoid file spam)
///
/// WHEN TO USE:
/// - VERBOSE: High-frequency gameplay (weapon firing, hits, every-frame updates)
/// - INFO: Significant events (level up, phase changes, unlocks)
/// - WARNING: Recoverable issues (missing optional assets, performance warnings)
/// - ERROR: Critical failures (missing required assets, exceptions)
/// </summary>
public static class DebugLog
{
    /// <summary>
    /// Verbose logging for hot paths (Update loops, collision checks).
    /// Only active with DEBUG_VERBOSE symbol defined.
    /// OUTPUT: Unity Console only (NOT written to log files to avoid spam)
    /// </summary>
    [System.Diagnostics.Conditional("DEBUG_VERBOSE")]
    public static void Verbose(string message, string category = null)
    {
        Debug.Log($"[VERBOSE] {message}");
        // NOT written to log file - would create 100+ KB logs per minute
    }

    /// <summary>
    /// Standard info logging. Only active in Editor.
    /// OUTPUT: Unity Console + Log File
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Info(string message, string category = null)
    {
        // Write to Unity Console
        Debug.Log(message);

        // ALSO write to log file (unified logging)
        if (PersistentLogger.IsInitialized)
            PersistentLogger.Info(message, category ?? "Game");
    }

    /// <summary>
    /// Warning logging. Active in Editor and Development builds.
    /// OUTPUT: Unity Console + Log File
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Warning(string message, string category = null)
    {
        // Write to Unity Console
        Debug.LogWarning(message);

        // ALSO write to log file (unified logging)
        if (PersistentLogger.IsInitialized)
            PersistentLogger.Warning(message, category ?? "Game");
    }

    /// <summary>
    /// Error logging. Always active (even in Release).
    /// OUTPUT: Unity Console + Log File
    /// </summary>
    public static void Error(string message, string category = null)
    {
        // Write to Unity Console
        Debug.LogError(message);

        // ALSO write to log file (unified logging)
        if (PersistentLogger.IsInitialized)
            PersistentLogger.Error(message, category ?? "Game", captureScreenshot: false);
    }
}
