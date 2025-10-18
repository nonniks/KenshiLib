using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

#nullable enable
namespace KenshiLib.Core;

public class MovedModsRegistry
{
  private static readonly string RegistryPath = "moved_mods_registry.json";
  private Dictionary<string, MovedModRecord> movedMods = new Dictionary<string, MovedModRecord>();

  public MovedModsRegistry() => this.Load();

  public void Load()
  {
    try
    {
      if (!File.Exists(MovedModsRegistry.RegistryPath))
        return;
      List<MovedModRecord> movedModRecordList = JsonSerializer.Deserialize<List<MovedModRecord>>(File.ReadAllText(MovedModsRegistry.RegistryPath), (JsonSerializerOptions) null);
      if (movedModRecordList == null)
        return;
      this.movedMods = Enumerable.ToDictionary<MovedModRecord, string, MovedModRecord>((IEnumerable<MovedModRecord>) movedModRecordList, (Func<MovedModRecord, string>) (r => r.ModName), (Func<MovedModRecord, MovedModRecord>) (r => r));
    }
    catch (Exception ex)
    {
    }
  }

  public void Save()
  {
    try
    {
      string str = JsonSerializer.Serialize<List<MovedModRecord>>(Enumerable.ToList<MovedModRecord>((IEnumerable<MovedModRecord>) this.movedMods.Values), new JsonSerializerOptions()
      {
        WriteIndented = true
      });
      File.WriteAllText(MovedModsRegistry.RegistryPath, str);
    }
    catch (Exception ex)
    {
      throw new Exception("Failed to save moved mods registry: " + ex.Message);
    }
  }

  public void RegisterMove(
    string modName,
    long workshopId,
    string gameDirPath,
    string workshopPath)
  {
    this.movedMods[modName] = new MovedModRecord()
    {
      ModName = modName,
      WorkshopId = workshopId,
      GameDirPath = gameDirPath,
      WorkshopPath = workshopPath,
      MovedDate = DateTime.Now
    };
    this.Save();
  }

  public void UnregisterMove(string modName)
  {
    if (!this.movedMods.ContainsKey(modName))
      return;
    this.movedMods.Remove(modName);
    this.Save();
  }

  public bool IsModMoved(string modName) => this.movedMods.ContainsKey(modName);

  public MovedModRecord? GetRecord(string modName)
  {
    MovedModRecord movedModRecord;
    return !this.movedMods.TryGetValue(modName, out movedModRecord) ? (MovedModRecord) null : movedModRecord;
  }

  public List<MovedModRecord> GetAllMovedMods()
  {
    return Enumerable.ToList<MovedModRecord>((IEnumerable<MovedModRecord>) this.movedMods.Values);
  }
}
