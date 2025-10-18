using System;
using System.IO;
using System.Text;

#nullable enable
namespace KenshiLib.Core;

public class TransactionLogger
{
  private static readonly string LogFilePath = "mod_migration_log.txt";
  private static readonly object lockObject = new object();

  public static void Log(
    TransactionLogger.OperationType operation,
    string modName,
    string details = "",
    bool success = true)
  {
    try
    {
      lock (TransactionLogger.lockObject)
      {
        string str1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string str2 = success ? "SUCCESS" : "FAILED";
        StringBuilder stringBuilder1 = new StringBuilder();
        stringBuilder1.AppendLine($"[{str1}] [{str2}] {operation}");
        stringBuilder1.AppendLine($"  Mod: {modName}");
        if (!string.IsNullOrEmpty(details))
        {
          stringBuilder1.AppendLine($"  Details: {details}");
        }
        stringBuilder1.AppendLine();
        File.AppendAllText(TransactionLogger.LogFilePath, stringBuilder1.ToString());
      }
    }
    catch (Exception ex)
    {
    }
  }

  public static void LogMove(
    string modName,
    long workshopId,
    string sourcePath,
    string destPath,
    bool success,
    string? error = null)
  {
    string details = $"Workshop ID: {workshopId}\n  Source: {sourcePath}\n  Destination: {destPath}";
    if (error != null)
      details = $"{details}\n  Error: {error}";
    TransactionLogger.Log(TransactionLogger.OperationType.MoveToGameDir, modName, details, success);
  }

  public static void LogRestore(
    string modName,
    string sourcePath,
    string destPath,
    bool success,
    string? error = null)
  {
    string details = $"Source: {sourcePath}\n  Destination: {destPath}";
    if (error != null)
      details = $"{details}\n  Error: {error}";
    TransactionLogger.Log(TransactionLogger.OperationType.RestoreToWorkshop, modName, details, success);
  }

  public static void LogBatchStart(string operationType, int modCount)
  {
    TransactionLogger.Log((TransactionLogger.OperationType) (operationType.Contains("Move") ? 2 : 4), $"Batch of {modCount} mods", "Operation: " + operationType);
  }

  public static void LogBatchComplete(string operationType, int successCount, int failCount)
  {
    TransactionLogger.Log((TransactionLogger.OperationType) (operationType.Contains("Move") ? 3 : 5), "Batch operation complete", $"Success: {successCount}, Failed: {failCount}", (failCount == 0 ? 1 : 0) != 0);
  }

  public static void LogConflict(string modName, string conflictPath, string resolution)
  {
    string details = $"Conflict path: {conflictPath}\n  Resolution: {resolution}";
    TransactionLogger.Log(TransactionLogger.OperationType.Conflict, modName, details);
  }

  public static void LogRollback(string modName, string reason)
  {
    TransactionLogger.Log(TransactionLogger.OperationType.Rollback, modName, "Reason: " + reason, false);
  }

  public static void LogError(string modName, string errorMessage, string? stackTrace = null)
  {
    string details = errorMessage;
    if (!string.IsNullOrEmpty(stackTrace))
      details = $"{details}\n  Stack trace: {stackTrace}";
    TransactionLogger.Log(TransactionLogger.OperationType.Error, modName, details, false);
  }

  public static string GetLogFilePath() => Path.GetFullPath(TransactionLogger.LogFilePath);

  public static void ClearLog()
  {
    try
    {
      lock (TransactionLogger.lockObject)
      {
        if (!File.Exists(TransactionLogger.LogFilePath))
          return;
        File.WriteAllText(TransactionLogger.LogFilePath, $"=== Transaction Log Cleared at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n\n");
      }
    }
    catch (Exception ex)
    {
    }
  }

  public static string GetRecentLog(int lineCount = 100)
  {
    try
    {
      if (!File.Exists(TransactionLogger.LogFilePath))
        return "No log file found.";
      string[] strArray1 = File.ReadAllLines(TransactionLogger.LogFilePath);
      int num = Math.Max(0, strArray1.Length - lineCount);
      string[] strArray2 = new string[Math.Min(lineCount, strArray1.Length)];
      Array.Copy((Array) strArray1, num, (Array) strArray2, 0, strArray2.Length);
      return string.Join(Environment.NewLine, strArray2);
    }
    catch (Exception ex)
    {
      return "Error reading log: " + ex.Message;
    }
  }

  public enum OperationType
  {
    MoveToGameDir,
    RestoreToWorkshop,
    BatchMoveStart,
    BatchMoveComplete,
    BatchRestoreStart,
    BatchRestoreComplete,
    Error,
    Conflict,
    Rollback,
  }
}
