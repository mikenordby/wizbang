using UnityEngine;
using System;
using System.IO;
using System.Text;

/// <summary>
/// Persistent file logger that writes timestamped logs to disk.
/// Creates a new log file each session in the project root.
/// Useful for debugging issues when Unity console is unreliable.
/// </summary>
public static class PersistentLogger
{
    private static string logFilePath;
    private static bool isInitialized = false;
    private static readonly object lockObj = new object();

    /// <summary>
    /// Check if logger is initialized (used by DebugLog for unified logging)
    /// </summary>
    public static bool IsInitialized => isInitialized;
    
    /// <summary>
    /// Initialize the logger. Creates a new log file with timestamp.
    /// Call this early in game initialization (e.g., from a bootstrap script).
    /// </summary>
    public static void Initialize()
    {
        if (isInitialized) return;
        
        // Create logs directory in project root (parent of Assets)
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string logsDir = Path.Combine(projectRoot, "AgentLogs");
        
        if (!Directory.Exists(logsDir))
        {
            Directory.CreateDirectory(logsDir);
        }
        
        // Create log file with timestamp
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logFilePath = Path.Combine(logsDir, $"game_log_{timestamp}.txt");
        
        isInitialized = true;
        
        // Write header
        StringBuilder header = new StringBuilder();
        header.AppendLine("=".PadRight(80, '='));
        header.AppendLine($"Game Log Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        header.AppendLine($"Unity Version: {Application.unityVersion}");
        header.AppendLine($"Platform: {Application.platform}");
        header.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        header.AppendLine("=".PadRight(80, '='));
        header.AppendLine();
        
        WriteToFile(header.ToString());
        
        Debug.Log($"[PersistentLogger] Initialized. Writing to: {logFilePath}");
    }
    
    /// <summary>
    /// Log an info message with timestamp
    /// </summary>
    public static void Info(string message, string category = null)
    {
        Log("INFO", message, category);
    }
    
    /// <summary>
    /// Log a warning message with timestamp
    /// </summary>
    public static void Warning(string message, string category = null)
    {
        Log("WARN", message, category);
    }
    
    /// <summary>
    /// Log an error message with timestamp
    /// </summary>
    public static void Error(string message, string category = null, bool captureScreenshot = false)
    {
        Log("ERROR", message, category);

        if (captureScreenshot)
        {
            CaptureScreenshot(category ?? "Error");
        }
    }
    
    /// <summary>
    /// Log with custom level
    /// </summary>
    public static void Log(string level, string message, string category = null)
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string categoryStr = category != null ? $"[{category}] " : "";
        string logLine = $"[{timestamp}] [{level.PadRight(5)}] {categoryStr}{message}";
        
        // Write to Unity console too
        if (level == "ERROR")
            Debug.LogError(logLine);
        else if (level == "WARN")
            Debug.LogWarning(logLine);
        else
            Debug.Log(logLine);
        
        // Write to file
        WriteToFile(logLine + "\n");
    }
    
    /// <summary>
    /// Write a separator line for readability
    /// </summary>
    public static void Separator(string label = null)
    {
        if (!isInitialized) Initialize();
        
        string line = label != null 
            ? $"\n--- {label} {"-".PadRight(70 - label.Length, '-')}\n"
            : "\n" + "-".PadRight(80, '-') + "\n";
        
        WriteToFile(line);
    }
    
    /// <summary>
    /// Thread-safe file write
    /// </summary>
    private static void WriteToFile(string content)
    {
        lock (lockObj)
        {
            try
            {
                File.AppendAllText(logFilePath, content);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PersistentLogger] Failed to write to log file: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Get the current log file path
    /// </summary>
    public static string GetLogFilePath()
    {
        if (!isInitialized) Initialize();
        return logFilePath;
    }

    /// <summary>
    /// Capture a screenshot and save it next to the log file
    /// </summary>
    public static void CaptureScreenshot(string label = "Screenshot")
    {
        if (!isInitialized) Initialize();

        try
        {
            string timestamp = System.DateTime.Now.ToString("HH-mm-ss-fff");
            string sanitizedLabel = label.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
            string screenshotFilename = $"screenshot_{sanitizedLabel}_{timestamp}.png";

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string logsDir = Path.Combine(projectRoot, "AgentLogs");
            string screenshotPath = Path.Combine(logsDir, screenshotFilename);

            // Unity's ScreenCapture.CaptureScreenshot saves relative to project root
            // We need to capture to the AgentLogs folder
            UnityEngine.ScreenCapture.CaptureScreenshot(screenshotPath);

            Log("INFO", $"Screenshot captured: {screenshotFilename}", "Screenshot");
        }
        catch (System.Exception ex)
        {
            Log("ERROR", $"Failed to capture screenshot: {ex.Message}", "Screenshot");
        }
    }
    
    /// <summary>
    /// Flush and close the log (optional, called on app quit)
    /// </summary>
    public static void Close()
    {
        if (!isInitialized) return;
        
        WriteToFile($"\n[{DateTime.Now:HH:mm:ss.fff}] [INFO ] Log closed.\n");
        isInitialized = false;
    }
}
