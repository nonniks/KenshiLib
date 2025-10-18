using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

#nullable enable
namespace KenshiLib.Core;

public class PlaysetRepository
{
  private readonly string _kenshiPath;
  private readonly string _playsetsDirectory;
  private readonly string _activePlaysetPath;

  public PlaysetRepository(string kenshiPath)
  {
    this._kenshiPath = kenshiPath ?? throw new ArgumentNullException(nameof (kenshiPath));
    this._playsetsDirectory = Path.Combine(kenshiPath, "data", "playsets");
    this._activePlaysetPath = Path.Combine(kenshiPath, "data", "mods.cfg");
    if (Directory.Exists(this._playsetsDirectory))
      return;
    Directory.CreateDirectory(this._playsetsDirectory);
    Console.WriteLine("[PlaysetRepository] Created playsets directory: " + this._playsetsDirectory);
  }

  public List<PlaysetInfo> GetAllPlaysets()
  {
    List<PlaysetInfo> allPlaysets = new List<PlaysetInfo>();
    try
    {
      foreach (string file in Directory.GetFiles(this._playsetsDirectory, "*.cfg"))
      {
        PlaysetInfo playsetInfo = this.GetPlaysetInfo(file);
        if (playsetInfo != null)
          allPlaysets.Add(playsetInfo);
      }
      allPlaysets = Enumerable.ToList<PlaysetInfo>((IEnumerable<PlaysetInfo>) Enumerable.OrderBy<PlaysetInfo, string>((IEnumerable<PlaysetInfo>) allPlaysets, (Func<PlaysetInfo, string>) (p => p.Name)));
      Console.WriteLine($"[PlaysetRepository] Found {allPlaysets.Count} playsets");
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error getting playsets: " + ex.Message);
    }
    return allPlaysets;
  }

  private PlaysetInfo? GetPlaysetInfo(string filePath)
  {
    try
    {
      if (!File.Exists(filePath))
        return (PlaysetInfo) null;
      FileInfo fileInfo = new FileInfo(filePath);
      int num = Enumerable.Count<string>((IEnumerable<string>) File.ReadAllLines(filePath), (Func<string, bool>) (line => !string.IsNullOrWhiteSpace(line)));
      return new PlaysetInfo()
      {
        Name = Path.GetFileNameWithoutExtension(filePath),
        FilePath = filePath,
        ModCount = num,
        CreatedDate = ((FileSystemInfo) fileInfo).CreationTime,
        ModifiedDate = ((FileSystemInfo) fileInfo).LastWriteTime,
        IsActive = false
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[PlaysetRepository] Error reading playset info from {filePath}: {ex.Message}");
      return (PlaysetInfo) null;
    }
  }

  public PlaysetInfo CreateNewPlayset(string name)
  {
    string str = !string.IsNullOrWhiteSpace(name) ? string.Join("_", name.Split(Path.GetInvalidFileNameChars())) : throw new ArgumentException("Playset name cannot be empty", nameof (name));
    string filePath = Path.Combine(this._playsetsDirectory, str + ".cfg");
    int num = 1;
    while (File.Exists(filePath))
    {
      filePath = Path.Combine(this._playsetsDirectory, $"{str}_{num}.cfg");
      ++num;
    }
    File.WriteAllText(filePath, string.Empty);
    Console.WriteLine("[PlaysetRepository] Created new playset: " + filePath);
    return this.GetPlaysetInfo(filePath);
  }

  public PlaysetInfo? DuplicatePlayset(string sourceFilePath, string newName)
  {
    if (!File.Exists(sourceFilePath))
    {
      Console.WriteLine("[PlaysetRepository] Source playset not found: " + sourceFilePath);
      return (PlaysetInfo) null;
    }
    try
    {
      string str = string.Join("_", newName.Split(Path.GetInvalidFileNameChars()));
      string filePath = Path.Combine(this._playsetsDirectory, str + ".cfg");
      int num = 1;
      while (File.Exists(filePath))
      {
        filePath = Path.Combine(this._playsetsDirectory, $"{str}_{num}.cfg");
        ++num;
      }
      File.Copy(sourceFilePath, filePath);
      Console.WriteLine($"[PlaysetRepository] Duplicated playset: {sourceFilePath} -> {filePath}");
      return this.GetPlaysetInfo(filePath);
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error duplicating playset: " + ex.Message);
      return (PlaysetInfo) null;
    }
  }

  public PlaysetInfo? RenamePlayset(string filePath, string newName)
  {
    if (!File.Exists(filePath))
    {
      Console.WriteLine("[PlaysetRepository] Playset not found: " + filePath);
      return (PlaysetInfo) null;
    }
    try
    {
      string filePath1 = Path.Combine(this._playsetsDirectory, string.Join("_", newName.Split(Path.GetInvalidFileNameChars())) + ".cfg");
      if (File.Exists(filePath1))
      {
        Console.WriteLine($"[PlaysetRepository] Playset with name '{newName}' already exists");
        return (PlaysetInfo) null;
      }
      File.Move(filePath, filePath1);
      Console.WriteLine($"[PlaysetRepository] Renamed playset: {filePath} -> {filePath1}");
      return this.GetPlaysetInfo(filePath1);
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error renaming playset: " + ex.Message);
      return (PlaysetInfo) null;
    }
  }

  public bool DeletePlayset(string filePath)
  {
    if (!File.Exists(filePath))
    {
      Console.WriteLine("[PlaysetRepository] Playset not found: " + filePath);
      return false;
    }
    try
    {
      Console.WriteLine("[PlaysetRepository] Attempting to delete playset file: " + filePath);
      File.Delete(filePath);
      if (File.Exists(filePath))
      {
        Console.WriteLine("[PlaysetRepository] WARNING: File still exists after delete attempt!");
        Thread.Sleep(100);
        if (File.Exists(filePath))
        {
          Console.WriteLine("[PlaysetRepository] ERROR: Failed to delete file: " + filePath);
          return false;
        }
      }
      Console.WriteLine("[PlaysetRepository] Successfully deleted playset: " + filePath);
      return true;
    }
    catch (UnauthorizedAccessException ex)
    {
      Console.WriteLine("[PlaysetRepository] Access denied when deleting playset: " + ((Exception) ex).Message);
      return false;
    }
    catch (IOException ex)
    {
      Console.WriteLine("[PlaysetRepository] IO error when deleting playset (file may be in use): " + ((Exception) ex).Message);
      return false;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Unexpected error deleting playset: " + ex.Message);
      Console.WriteLine("[PlaysetRepository] Stack trace: " + ex.StackTrace);
      return false;
    }
  }

  public PlaysetInfo? ImportPlayset(string sourceFilePath, string? playsetName = null)
  {
    if (!File.Exists(sourceFilePath))
    {
      Console.WriteLine("[PlaysetRepository] Source file not found: " + sourceFilePath);
      return (PlaysetInfo) null;
    }
    try
    {
      string str = string.Join("_", (playsetName ?? Path.GetFileNameWithoutExtension(sourceFilePath)).Split(Path.GetInvalidFileNameChars()));
      string filePath = Path.Combine(this._playsetsDirectory, str + ".cfg");
      int num = 1;
      while (File.Exists(filePath))
      {
        filePath = Path.Combine(this._playsetsDirectory, $"{str}_{num}.cfg");
        ++num;
      }
      File.Copy(sourceFilePath, filePath);
      Console.WriteLine($"[PlaysetRepository] Imported playset: {sourceFilePath} -> {filePath}");
      return this.GetPlaysetInfo(filePath);
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error importing playset: " + ex.Message);
      return (PlaysetInfo) null;
    }
  }

  public bool ExportPlayset(string sourceFilePath, string destinationPath)
  {
    if (!File.Exists(sourceFilePath))
    {
      Console.WriteLine("[PlaysetRepository] Source playset not found: " + sourceFilePath);
      return false;
    }
    try
    {
      File.Copy(sourceFilePath, destinationPath, true);
      Console.WriteLine($"[PlaysetRepository] Exported playset: {sourceFilePath} -> {destinationPath}");
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error exporting playset: " + ex.Message);
      return false;
    }
  }

  public bool ActivatePlayset(string playsetFilePath)
  {
    if (!File.Exists(playsetFilePath))
    {
      Console.WriteLine("[PlaysetRepository] Playset not found: " + playsetFilePath);
      return false;
    }
    try
    {
      if (File.Exists(this._activePlaysetPath))
      {
        string str = Path.Combine(this._playsetsDirectory, $"Active_{DateTime.Now:yyyyMMdd_HHmmss}.cfg");
        File.Copy(this._activePlaysetPath, str, true);
        Console.WriteLine("[PlaysetRepository] Backed up current active playset to: " + str);
      }
      File.Copy(playsetFilePath, this._activePlaysetPath, true);
      Console.WriteLine("[PlaysetRepository] Activated playset: " + playsetFilePath);
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error activating playset: " + ex.Message);
      return false;
    }
  }

  public PlaysetInfo? SaveActivePlaysetAs(string name)
  {
    if (!File.Exists(this._activePlaysetPath))
    {
      Console.WriteLine("[PlaysetRepository] Active playset (mods.cfg) not found");
      return (PlaysetInfo) null;
    }
    try
    {
      string str = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
      string filePath = Path.Combine(this._playsetsDirectory, str + ".cfg");
      int num = 1;
      while (File.Exists(filePath))
      {
        filePath = Path.Combine(this._playsetsDirectory, $"{str}_{num}.cfg");
        ++num;
      }
      File.Copy(this._activePlaysetPath, filePath);
      Console.WriteLine("[PlaysetRepository] Saved active playset as: " + filePath);
      return this.GetPlaysetInfo(filePath);
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error saving active playset: " + ex.Message);
      return (PlaysetInfo) null;
    }
  }

  public string GetActivePlaysetName() => "Active";

  public List<string> LoadPlaysetMods(string playsetFilePath)
  {
    List<string> stringList = new List<string>();
    if (!File.Exists(playsetFilePath))
    {
      Console.WriteLine("[PlaysetRepository] Playset file not found: " + playsetFilePath);
      return stringList;
    }
    try
    {
      foreach (string readAllLine in File.ReadAllLines(playsetFilePath))
      {
        string str = readAllLine.Trim();
        if (!string.IsNullOrWhiteSpace(str))
          stringList.Add(str);
      }
      Console.WriteLine($"[PlaysetRepository] Loaded {stringList.Count} mods from playset: {playsetFilePath}");
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error loading playset mods: " + ex.Message);
    }
    return stringList;
  }

  public bool SaveModsToPlayset(string playsetFilePath, List<string> modNames)
  {
    try
    {
      File.WriteAllLines(playsetFilePath, (IEnumerable<string>) modNames);
      Console.WriteLine($"[PlaysetRepository] Saved {modNames.Count} mods to playset: {playsetFilePath}");
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error saving mods to playset: " + ex.Message);
      return false;
    }
  }

  public List<PlaysetModEntry> LoadPlaysetModsWithState(string playsetFilePath)
  {
    List<PlaysetModEntry> playsetModEntryList = new List<PlaysetModEntry>();
    if (!File.Exists(playsetFilePath))
    {
      Console.WriteLine("[PlaysetRepository] Playset file not found: " + playsetFilePath);
      return playsetModEntryList;
    }
    try
    {
      foreach (string readAllLine in File.ReadAllLines(playsetFilePath))
      {
        string str1 = readAllLine.Trim();
        if (!string.IsNullOrWhiteSpace(str1))
        {
          if (str1.StartsWith("#"))
          {
            string str2 = str1.Substring(1).Trim();
            if (!string.IsNullOrWhiteSpace(str2))
              playsetModEntryList.Add(new PlaysetModEntry()
              {
                ModName = str2,
                IsEnabled = false,
                OriginalLine = readAllLine
              });
          }
          else
            playsetModEntryList.Add(new PlaysetModEntry()
            {
              ModName = str1,
              IsEnabled = true,
              OriginalLine = readAllLine
            });
        }
      }
      Console.WriteLine($"[PlaysetRepository] Loaded {playsetModEntryList.Count} mod entries from playset: {playsetFilePath}");
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error loading playset mods with state: " + ex.Message);
    }
    return playsetModEntryList;
  }

  public bool SaveModsToPlaysetWithState(string playsetFilePath, IEnumerable<ModInfo> mods)
  {
    try
    {
      List<string> stringList = new List<string>();
      foreach (ModInfo modInfo in (IEnumerable<ModInfo>) Enumerable.OrderBy<ModInfo, int>(mods, (Func<ModInfo, int>) (m => m.LoadOrder)))
      {
        if (modInfo.IsEnabled)
          stringList.Add(modInfo.Name);
        else
          stringList.Add("#" + modInfo.Name);
      }
      File.WriteAllLines(playsetFilePath, (IEnumerable<string>) stringList);
      Console.WriteLine($"[PlaysetRepository] Saved {Enumerable.Count<ModInfo>(mods)} mods to playset ({Enumerable.Count<ModInfo>(mods, (Func<ModInfo, bool>) (m => m.IsEnabled))} enabled, {Enumerable.Count<ModInfo>(mods, (Func<ModInfo, bool>) (m => !m.IsEnabled))} disabled): {playsetFilePath}");
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetRepository] Error saving mods to playset with state: " + ex.Message);
      return false;
    }
  }
}
