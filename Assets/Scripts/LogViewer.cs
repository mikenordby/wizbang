using UnityEngine;
using System.IO;
using System.Linq;

/// <summary>
/// Editor utility to view persistent logs.
/// Add context menu items to easily access log files.
/// </summary>
public class LogViewer : MonoBehaviour
{
    [ContextMenu("Open Latest Log File")]
    private void OpenLatestLog()
    {
        string logPath = PersistentLogger.GetLogFilePath();
        if (File.Exists(logPath))
        {
            Application.OpenURL("file:///" + logPath);
            Debug.Log($"Opened log file: {logPath}");
        }
        else
        {
            Debug.LogWarning("No log file found. Run the game first to generate logs.");
        }
    }
    
    [ContextMenu("Show Log File Path")]
    private void ShowLogPath()
    {
        string logPath = PersistentLogger.GetLogFilePath();
        Debug.Log($"Current log file: {logPath}");
        Debug.Log($"Logs directory: {Path.GetDirectoryName(logPath)}");
    }
    
    [ContextMenu("Print Latest Log to Console")]
    private void PrintLatestLog()
    {
        string logPath = PersistentLogger.GetLogFilePath();
        if (File.Exists(logPath))
        {
            string content = File.ReadAllText(logPath);
            Debug.Log("=== LATEST LOG FILE ===\n" + content);
        }
        else
        {
            Debug.LogWarning("No log file found.");
        }
    }
    
    [ContextMenu("Open Logs Directory")]
    private void OpenLogsDirectory()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string logsDir = Path.Combine(projectRoot, "AgentLogs");
        
        if (Directory.Exists(logsDir))
        {
            Application.OpenURL("file:///" + logsDir);
            Debug.Log($"Opened logs directory: {logsDir}");
        }
        else
        {
            Debug.LogWarning("Logs directory doesn't exist yet. Run the game first.");
        }
    }
}
