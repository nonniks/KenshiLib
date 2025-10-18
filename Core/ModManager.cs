using KenshiLib.Translation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

#nullable enable
namespace KenshiLib.Core;

public class ModManager
{
  private readonly ReverseEngineer _re;
  private readonly object _lock = new object();
  private static string? steamInstallPath;
  private static string? kenshiPath;
  public static string? gamedirModsPath;
  public static string? workshopModsPath;

  public static string? KenshiPath => ModManager.kenshiPath;

  public ModManager(ReverseEngineer re)
  {
    this.solvePaths();
    this._re = re ?? throw new ArgumentNullException(nameof (re));
  }

  [SupportedOSPlatform("windows")]
  private static string FindSteamInstallPath()
  {
    string steamInstallPath1 = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", (object) null) as string;
    if (!string.IsNullOrEmpty(steamInstallPath1))
      return steamInstallPath1;
    string steamInstallPath2 = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", (object) null) as string;
    if (!string.IsNullOrEmpty(steamInstallPath2))
      return steamInstallPath2;
    return Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", (object) null) is string str ? str : string.Empty;
  }

  private static string? FindKenshiInstallDir(string steamPath)
  {
    if (!string.IsNullOrEmpty(steamPath))
    {
      string kenshiInstallDir = Path.Combine(steamPath, "steamapps", "common", "Kenshi");
      if (Directory.Exists(kenshiInstallDir))
        return kenshiInstallDir;
    }
    if (!string.IsNullOrEmpty(steamPath))
    {
      string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
      if (File.Exists(vdfPath))
      {
        foreach (string str in ModManager.ParseLibraryFoldersVdf(vdfPath))
        {
          string kenshiInstallDir = Path.Combine(str, "steamapps", "common", "Kenshi");
          if (Directory.Exists(kenshiInstallDir))
            return kenshiInstallDir;
        }
      }
    }
    string[] strArray1 = new string[5]
    {
      "C:",
      "D:",
      "E:",
      "F:",
      "G:"
    };
    string[] strArray2 = new string[4]
    {
      "SteamLibrary\\steamapps\\common\\Kenshi",
      "Steam\\steamapps\\common\\Kenshi",
      "Program Files (x86)\\Steam\\steamapps\\common\\Kenshi",
      "Program Files\\Steam\\steamapps\\common\\Kenshi"
    };
    foreach (string str1 in strArray1)
    {
      foreach (string str2 in strArray2)
      {
        string kenshiInstallDir = Path.Combine(str1, str2);
        if (Directory.Exists(kenshiInstallDir))
          return kenshiInstallDir;
      }
    }
    Console.WriteLine("[ModManager] Could not auto-detect Kenshi installation.");
    return (string) null;
  }

  private static List<string> ParseLibraryFoldersVdf(string vdfPath)
  {
    List<string> libraryFoldersVdf = new List<string>();
    try
    {
      foreach (string readAllLine in File.ReadAllLines(vdfPath))
      {
        if (readAllLine.Contains("\"path\""))
        {
          string[] strArray = readAllLine.Split(new char[1] { '"' }, StringSplitOptions.RemoveEmptyEntries);
          if (strArray.Length >= 3)
          {
            string str = strArray[2].Trim().Replace("\\\\", "\\");
            if (Directory.Exists(str))
              libraryFoldersVdf.Add(str);
          }
        }
      }
    }
    catch (Exception ex)
    {
    }
    return libraryFoldersVdf;
  }

  [SupportedOSPlatform("windows")]
  public void solvePaths()
  {
    ModManager.steamInstallPath = ModManager.FindSteamInstallPath();
    ModManager.kenshiPath = ModManager.FindKenshiInstallDir(ModManager.steamInstallPath);
    if (string.IsNullOrEmpty(ModManager.kenshiPath))
    {
      Console.WriteLine("[ModManager] Kenshi installation not found!");
    }
    else
    {
      ModManager.gamedirModsPath = Path.Combine(ModManager.kenshiPath, "mods");
      ModManager.workshopModsPath = ModManager.FindWorkshopPath(ModManager.steamInstallPath, ModManager.kenshiPath);
    }
  }

  private static string? FindWorkshopPath(string? mainSteamPath, string kenshiPath)
  {
    if (!string.IsNullOrEmpty(kenshiPath) && kenshiPath.Contains("steamapps"))
    {
      int num = kenshiPath.IndexOf("steamapps", StringComparison.OrdinalIgnoreCase);
      if (num > 0)
      {
        string str = kenshiPath.Substring(0, num);
        string workshopPath = Path.Combine(str, "steamapps", "workshop", "content", "233860");
        if (Directory.Exists(workshopPath))
          return workshopPath;
      }
    }
    if (!string.IsNullOrEmpty(mainSteamPath))
    {
      string workshopPath = Path.Combine(mainSteamPath, "steamapps", "workshop", "content", "233860");
      if (Directory.Exists(workshopPath))
        return workshopPath;
    }
    if (!string.IsNullOrEmpty(mainSteamPath))
    {
      string vdfPath = Path.Combine(mainSteamPath, "steamapps", "libraryfolders.vdf");
      if (File.Exists(vdfPath))
      {
        foreach (string str in ModManager.ParseLibraryFoldersVdf(vdfPath))
        {
          string workshopPath = Path.Combine(str, "steamapps", "workshop", "content", "233860");
          if (Directory.Exists(workshopPath))
            return workshopPath;
        }
      }
    }
    string[] strArray1 = new string[5]
    {
      "C:",
      "D:",
      "E:",
      "F:",
      "G:"
    };
    string[] strArray2 = new string[4]
    {
      "SteamLibrary\\steamapps\\workshop\\content\\233860",
      "Steam\\steamapps\\workshop\\content\\233860",
      "Program Files (x86)\\Steam\\steamapps\\workshop\\content\\233860",
      "Program Files\\Steam\\steamapps\\workshop\\content\\233860"
    };
    foreach (string str1 in strArray1)
    {
      foreach (string str2 in strArray2)
      {
        string workshopPath = Path.Combine(str1, str2);
        if (Directory.Exists(workshopPath))
          return workshopPath;
      }
    }
    return (string) null;
  }

  public List<string> LoadGameDirMods()
  {
    List<string> stringList = new List<string>();
    if (string.IsNullOrEmpty(ModManager.gamedirModsPath) || !Directory.Exists(ModManager.gamedirModsPath))
    {
      Console.WriteLine("[ModManager] gamedir folder not found!");
      return stringList;
    }
    foreach (string directory in Directory.GetDirectories(ModManager.gamedirModsPath))
    {
      foreach (string file in Directory.GetFiles(directory, "*.mod"))
        stringList.Add(Path.GetFileName(file));
    }
    return stringList;
  }

  public List<string> LoadWorkshopMods()
  {
    List<string> stringList = new List<string>();
    if (!Directory.Exists(ModManager.workshopModsPath))
    {
      Console.WriteLine("[ModManager] workshop folder not found!");
      return stringList;
    }
    foreach (string directory in Directory.GetDirectories(ModManager.workshopModsPath))
    {
      foreach (string file in Directory.GetFiles(directory, "*.mod"))
      {
        string str = Path.Combine(((FileSystemInfo) new DirectoryInfo(Path.GetDirectoryName(file))).Name, Path.GetFileName(file));
        stringList.Add(str);
      }
    }
    return stringList;
  }

  public List<string> LoadSelectedMods()
  {
    List<string> stringList = new List<string>();
    if (string.IsNullOrEmpty(ModManager.kenshiPath))
      return stringList;
    string str = Path.Combine(ModManager.kenshiPath, "data", "mods.cfg");
    if (!File.Exists(str))
    {
      Console.WriteLine("[ModManager] mods.cfg not found!");
      return stringList;
    }
    foreach (string readAllLine in File.ReadAllLines(str))
    {
      if (!string.IsNullOrWhiteSpace(readAllLine))
        stringList.Add(readAllLine.Trim());
    }
    return stringList;
  }

  public void LoadModFile(string modPath)
  {
    if (string.IsNullOrEmpty(modPath) || !File.Exists(modPath))
    {
      Console.WriteLine("[ModManager] Mod file path invalid: " + modPath);
    }
    else
    {
      lock (this._lock)
        this._re.LoadModFile(modPath);
    }
  }

  public ReverseEngineer GetReverseEngineer() => this._re;

  public async Task<List<ModInfo>> LoadAllModsWithDetailsAsync(bool includeTranslationInfo = false)
  {
    List<ModInfo> allMods = new List<ModInfo>();
    List<string> stringList1 = this.LoadGameDirMods();
    List<string> stringList2 = this.LoadWorkshopMods();

    bool initialPlaysetExists = !string.IsNullOrEmpty(ModManager.kenshiPath) &&
      File.Exists(Path.Combine(ModManager.kenshiPath, "data", "playsets", "Initial Playset.cfg"));
    List<string> stringList3 = initialPlaysetExists ? new List<string>() : this.LoadSelectedMods();

    if (initialPlaysetExists)
    {
      Console.WriteLine("[ModManager] InitialPlayset exists - NOT reading mods.cfg");
    }
    else
    {
      Console.WriteLine($"[ModManager] InitialPlayset doesn't exist - reading {stringList3.Count} mods from mods.cfg");
    }

    Playset playset = new PlaysetManager(ModManager.kenshiPath).LoadActivePlayset();
    Dictionary<string, ModItem> dictionary = new Dictionary<string, ModItem>();
    foreach (string name in stringList3)
    {
      if (!dictionary.ContainsKey(name))
        dictionary[name] = new ModItem(name);
      dictionary[name].Selected = true;
    }
    foreach (string name in stringList1)
    {
      if (!dictionary.ContainsKey(name))
        dictionary[name] = new ModItem(name);
      dictionary[name].InGameDir = true;
    }
    foreach (string str in stringList2)
    {
      string directoryName = Path.GetDirectoryName(str);
      if (directoryName != null)
      {
        string fileName = Path.GetFileName(str);
        if (!dictionary.ContainsKey(fileName))
          dictionary[fileName] = new ModItem(fileName);
        dictionary[fileName].WorkshopId = Convert.ToInt64(directoryName);
      }
    }
    foreach (ModItem modItem in dictionary.Values)
    {
      ModItem mod = modItem;
      ModInfo modInfo = new ModInfo()
      {
        Name = mod.Name,
        FilePath = mod.getModFilePath(),
        InGameDir = mod.InGameDir,
        InWorkshop = mod.WorkshopId > 0L,
        WorkshopId = mod.WorkshopId,
        IsMarkedSelected = mod.Selected
      };
      if (modInfo.FilePath != null && File.Exists(modInfo.FilePath))
      {
        FileInfo fileInfo = new FileInfo(modInfo.FilePath);
        modInfo.FileSize = fileInfo.Length;
        modInfo.LastModified = ((FileSystemInfo) fileInfo).LastWriteTime;
        string directoryName = Path.GetDirectoryName(modInfo.FilePath);
        string withoutExtension = Path.GetFileNameWithoutExtension(modInfo.Name);
        if (directoryName != null)
        {
          string str = Path.Combine(directoryName, $"_{withoutExtension}.img");
          if (File.Exists(str))
            modInfo.ImagePath = str;
        }
      }
      PlaysetEntry playsetEntry = Enumerable.FirstOrDefault<PlaysetEntry>((IEnumerable<PlaysetEntry>) playset.Mods, (Func<PlaysetEntry, bool>) (m => m.ModName.Equals(mod.Name, StringComparison.OrdinalIgnoreCase)));
      if (playsetEntry != null)
      {
        modInfo.IsEnabled = true;
        modInfo.LoadOrder = playsetEntry.LoadOrder;
      }
      await this.LoadModMetadataAsync(modInfo);
      if (includeTranslationInfo)
        await this.LoadTranslationInfoAsync(modInfo);
      allMods.Add(modInfo);
      modInfo = (ModInfo) null;
    }
    Console.WriteLine($"[ModManager] Loaded {allMods.Count} mods (includeTranslationInfo={includeTranslationInfo})");
    List<ModInfo> modInfoList = allMods;
    allMods = (List<ModInfo>) null;
    playset = (Playset) null;
    return modInfoList;
  }

  private async Task LoadModMetadataAsync(ModInfo modInfo)
  {
    if (string.IsNullOrEmpty(modInfo.FilePath) || !File.Exists(modInfo.FilePath))
      return;
    await Task.Run((Action) (() =>
    {
      try
      {
        lock (this._lock)
        {
          this._re.LoadModFile(modInfo.FilePath, 0);
          if (this._re.modData.Header == null)
            return;
          modInfo.Author = this._re.modData.Header.Author ?? "";
          modInfo.Description = this._re.modData.Header.Description ?? "";
          modInfo.Dependencies = this._re.modData.Header.Dependencies ?? "";
          modInfo.References = this._re.modData.Header.References ?? "";
          modInfo.ModVersion = this._re.modData.Header.ModVersion;
          modInfo.FileType = this._re.modData.Header.FileType;
          modInfo.RecordCount = this._re.modData.Header.RecordCount;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[ModManager] Error loading metadata for {modInfo.Name}: {ex.Message}");
      }
    }));
  }

  private async Task LoadTranslationInfoAsync(ModInfo modInfo)
  {
    if (string.IsNullOrEmpty(modInfo.FilePath) || !File.Exists(modInfo.FilePath))
      return;
    await Task.Run((Action) (() =>
    {
      try
      {
        lock (this._lock)
        {
          this._re.LoadModFile(modInfo.FilePath, 50);
          Tuple<string, string> modSummary = this._re.getModSummary();
          modInfo.DetectedLanguage = $"{this.DetectLanguageFor(modSummary.Item1)}|{this.DetectLanguageFor(modSummary.Item2)}";
        }
        string dictFilePath = modInfo.GetDictFilePath();
        if (dictFilePath != null && File.Exists(dictFilePath))
        {
          modInfo.HasDictionary = true;
          modInfo.TranslationProgress = (int) TranslationDictionary.GetTranslationProgress(dictFilePath);
          string str = File.ReadAllText(dictFilePath);
          string[] strArray1 = new string[1]{ "|_END_|" };
          modInfo.StringCount = Enumerable.Count<string>((IEnumerable<string>) Enumerable.ToArray<string>(Enumerable.Where<string>(Enumerable.Select<string, string>((IEnumerable<string>) str.Split(strArray1, StringSplitOptions.RemoveEmptyEntries), (Func<string, string>) (s => s.Trim(new char[4]
          {
            '\r',
            '\n',
            ' ',
            '\t'
          }))), (Func<string, bool>) (l => !string.IsNullOrWhiteSpace(l)))), (Func<string, bool>) (line =>
          {
            string[] strArray2 = line.Split(new string[1]
            {
              "|_SEP_|"
            }, StringSplitOptions.None);
            if (strArray2.Length < 2)
              return false;
            string key = strArray2[0];
            return !TranslationDictionary.IsTechnicalName(strArray2[1], key);
          }));
        }
        string backupFilePath = modInfo.GetBackupFilePath();
        modInfo.HasBackup = backupFilePath != null && File.Exists(backupFilePath);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[ModManager] Error loading translation info for {modInfo.Name}: {ex.Message}");
      }
    }));
  }

  private string DetectLanguageFor(string text) => "___";

  // Public helper methods for Settings UI
  [SupportedOSPlatform("windows")]
  public static string? FindKenshiInstallDir()
  {
    string steamPath = FindSteamInstallPath();
    return FindKenshiInstallDir(steamPath);
  }

  [SupportedOSPlatform("windows")]
  public static string? FindWorkshopPath()
  {
    if (string.IsNullOrEmpty(steamInstallPath))
      steamInstallPath = FindSteamInstallPath();
    return FindWorkshopPath(steamInstallPath, kenshiPath);
  }

  public static void SetKenshiPath(string newKenshiPath)
  {
    if (string.IsNullOrEmpty(newKenshiPath))
      return;

    kenshiPath = newKenshiPath;
    gamedirModsPath = Path.Combine(kenshiPath, "mods");
    Console.WriteLine($"[ModManager] Kenshi path set to: {kenshiPath}");
  }
}
