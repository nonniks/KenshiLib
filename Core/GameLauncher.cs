using System;
using System.Diagnostics;
using System.IO;

#nullable enable
namespace KenshiLib.Core;

public class GameLauncher
{
  private readonly string _kenshiPath;

  public GameLauncher(string kenshiPath)
  {
    this._kenshiPath = kenshiPath ?? throw new ArgumentNullException(nameof (kenshiPath));
  }

  public bool LaunchGame(bool use64bit = true)
  {
    string str1 = use64bit ? "kenshi_x64.exe" : "kenshi.exe";
    string str2 = Path.Combine(this._kenshiPath, str1);
    if (!File.Exists(str2))
    {
      Console.WriteLine("[GameLauncher] Game executable not found: " + str2);
      return false;
    }
    try
    {
      Process process = new Process()
      {
        StartInfo = new ProcessStartInfo()
        {
          FileName = str2,
          WorkingDirectory = this._kenshiPath,
          UseShellExecute = true
        }
      };
      int num = process.Start() ? 1 : 0;
      if (num != 0)
        Console.WriteLine($"[GameLauncher] Successfully launched Kenshi: {str1} (PID: {process.Id})");
      else
        Console.WriteLine("[GameLauncher] Failed to start process: " + str1);
      return num != 0;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[GameLauncher] Exception launching game: " + ex.Message);
      return false;
    }
  }

  public bool IsGameRunning()
  {
    try
    {
      Process[] processesByName1 = Process.GetProcessesByName("kenshi_x64");
      if (processesByName1.Length != 0)
      {
        Console.WriteLine($"[GameLauncher] Kenshi (64-bit) is running, PID: {processesByName1[0].Id}");
        return true;
      }
      Process[] processesByName2 = Process.GetProcessesByName("kenshi");
      if (processesByName2.Length == 0)
        return false;
      Console.WriteLine($"[GameLauncher] Kenshi (32-bit) is running, PID: {processesByName2[0].Id}");
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[GameLauncher] Error checking if game is running: " + ex.Message);
      return false;
    }
  }

  public int LaunchAndWait(bool use64bit = true)
  {
    string str1 = use64bit ? "kenshi_x64.exe" : "kenshi.exe";
    string str2 = Path.Combine(this._kenshiPath, str1);
    if (!File.Exists(str2))
    {
      Console.WriteLine("[GameLauncher] Game executable not found: " + str2);
      return -1;
    }
    try
    {
      Process process = new Process()
      {
        StartInfo = new ProcessStartInfo()
        {
          FileName = str2,
          WorkingDirectory = this._kenshiPath,
          UseShellExecute = true
        }
      };
      process.Start();
      Console.WriteLine($"[GameLauncher] Launched Kenshi: {str1} (PID: {process.Id}), waiting for exit...");
      process.WaitForExit();
      int exitCode = process.ExitCode;
      Console.WriteLine($"[GameLauncher] Kenshi exited with code: {exitCode}");
      return exitCode;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[GameLauncher] Exception launching game: " + ex.Message);
      return -1;
    }
  }

  public string GetGameExecutablePath(bool use64bit = true)
  {
    return Path.Combine(this._kenshiPath, use64bit ? "kenshi_x64.exe" : "kenshi.exe");
  }

  public bool ValidateInstallation()
  {
    if (!File.Exists(this.GetGameExecutablePath()) && !File.Exists(this.GetGameExecutablePath(false)))
    {
      Console.WriteLine("[GameLauncher] No Kenshi executable found");
      return false;
    }
    string str = Path.Combine(this._kenshiPath, "data");
    if (!Directory.Exists(str))
    {
      Console.WriteLine("[GameLauncher] Data folder not found");
      return false;
    }
    if (!File.Exists(Path.Combine(str, "mods.cfg")))
      Console.WriteLine("[GameLauncher] mods.cfg not found (game might not have been run yet)");
    Console.WriteLine("[GameLauncher] Kenshi installation validated successfully");
    return true;
  }
}
