using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// Collects all console log messages during a play session
/// and writes them to a file upon application quit.
/// To use, attach this script to a persistent GameObject in your scene.
/// </summary>
public class LogCollector : MonoBehaviour
{
    private readonly StringBuilder _logBuilder = new StringBuilder();
    private string _logFilePath;
    private bool _hasWrittenLogs = false;

    void Awake()
    {
        // Make this object persistent across all scenes
        DontDestroyOnLoad(gameObject);

        // Determine the path for the log file (in the project's root directory)
        _logFilePath = Path.Combine(Application.dataPath, "..", "session_logs.txt");

        // Subscribe to the log message event
        Application.logMessageReceived += HandleLog;

        // Add a header for the new session
        _logBuilder.AppendLine($"--- New Log Session: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
        _logBuilder.AppendLine("-----------------------------------------------------");
        _logBuilder.AppendLine(); // Add a blank line for spacing

        // Log a confirmation message to the console to show the script is active
        Debug.Log($"[LogCollector] Initialized. Logs will be saved to: {_logFilePath}");
    }

    void OnDisable()
    {
        // This is often called more reliably than OnApplicationQuit in the editor
        WriteLogsToFile();
    }

    void OnApplicationQuit()
    {
        WriteLogsToFile();
    }

    void OnDestroy()
    {
        // Unsubscribe when the object is destroyed to prevent memory leaks
        Application.logMessageReceived -= HandleLog;
        WriteLogsToFile(); // Final fallback
    }

    private void WriteLogsToFile()
    {
        // Prevent writing the file multiple times
        if (_hasWrittenLogs)
        {
            return;
        }

        // In case there are no logs, don't create an empty file
        if (_logBuilder.Length <= 0)
        {
            return;
        }

        try
        {
            _logBuilder.AppendLine(); // Add a blank line for spacing
            _logBuilder.AppendLine($"--- Session Ended: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
            File.WriteAllText(_logFilePath, _logBuilder.ToString());

            // Mark as written and log success
            _hasWrittenLogs = true;
            Debug.Log($"[LogCollector] Logs successfully saved to: {_logFilePath}");
            
            // Also print the collected logs to the Unity console as requested
            Debug.Log("[LogCollector] Collected Session Logs:\n" + _logBuilder.ToString());
        }
        catch (System.Exception e)
        {
            // If logging to file fails, log the error to the Unity console itself
            Debug.LogError($"[LogCollector] Failed to write log file at {_logFilePath}. Error: {e.Message}");
        }
    }

    /// <summary>
    /// Formats and appends a log message to the string builder.
    /// </summary>
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        _logBuilder.AppendFormat("[{0}][{1}] {2}\n", System.DateTime.Now.ToString("HH:mm:ss"), type, logString);
        
        // Include stack trace for more severe log types for better debugging
        if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            // Add a light indentation to the stack trace for readability
            _logBuilder.Append("  ");
            _logBuilder.AppendLine(stackTrace.Replace("\n", "\n  "));
        }
        
        _logBuilder.AppendLine(); // Add a blank line for better separation between entries
    }
}
