using UnityEngine;

/// <summary>
/// Conditional debug logging that compiles out in builds.
/// Uses [Conditional] attribute to remove calls entirely in Release builds.
/// </summary>
public static class DebugLog
{
    /// <summary>
    /// Verbose logging for hot paths (Update loops, collision checks).
    /// Only active with DEBUG_VERBOSE symbol defined.
    /// </summary>
    [System.Diagnostics.Conditional("DEBUG_VERBOSE")]
    public static void Verbose(string message)
    {
        Debug.Log($"[VERBOSE] {message}");
    }
    
    /// <summary>
    /// Standard info logging. Only active in Editor.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Info(string message)
    {
        Debug.Log(message);
    }
    
    /// <summary>
    /// Warning logging. Active in Editor and Development builds.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Warning(string message)
    {
        Debug.LogWarning(message);
    }
    
    /// <summary>
    /// Error logging. Always active (even in Release).
    /// </summary>
    public static void Error(string message)
    {
        Debug.LogError(message);
    }
}
