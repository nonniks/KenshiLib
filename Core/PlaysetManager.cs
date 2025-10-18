using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable
namespace KenshiLib.Core;

public class PlaysetManager
{
  private readonly string _modsConfigPath;
  private readonly string _kenshiPath;

  public PlaysetManager(string kenshiPath)
  {
    this._kenshiPath = kenshiPath ?? throw new ArgumentNullException(nameof (kenshiPath));
    this._modsConfigPath = Path.Combine(kenshiPath, "data", "mods.cfg");
  }

  public Playset LoadActivePlayset()
  {
    Playset playset = new Playset() { Name = "Active" };
    if (!File.Exists(this._modsConfigPath))
    {
      Console.WriteLine("[PlaysetManager] mods.cfg not found at: " + this._modsConfigPath);
      return playset;
    }
    try
    {
      string[] strArray = File.ReadAllLines(this._modsConfigPath);
      for (int index = 0; index < strArray.Length; ++index)
      {
        string str = strArray[index].Trim();
        if (!string.IsNullOrWhiteSpace(str))
          playset.Mods.Add(new PlaysetEntry()
          {
            ModName = str,
            LoadOrder = index
          });
      }
      Console.WriteLine($"[PlaysetManager] Loaded {playset.Mods.Count} mods from active playset");
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetManager] Error loading playset: " + ex.Message);
    }
    return playset;
  }

  public void SavePlayset(Playset playset)
  {
    if (playset == null)
      throw new ArgumentNullException(nameof (playset));
    try
    {
      string directoryName = Path.GetDirectoryName(this._modsConfigPath);
      if (directoryName != null && !Directory.Exists(directoryName))
        Directory.CreateDirectory(directoryName);
      string[] array = Enumerable.ToArray<string>(Enumerable.Select<PlaysetEntry, string>((IEnumerable<PlaysetEntry>) Enumerable.OrderBy<PlaysetEntry, int>((IEnumerable<PlaysetEntry>) playset.Mods, (Func<PlaysetEntry, int>) (m => m.LoadOrder)), (Func<PlaysetEntry, string>) (m => m.ModName)));
      File.WriteAllLines(this._modsConfigPath, array);
      Console.WriteLine($"[PlaysetManager] Saved {array.Length} mods to playset");
    }
    catch (Exception ex)
    {
      Console.WriteLine("[PlaysetManager] Error saving playset: " + ex.Message);
      throw;
    }
  }

  public void EnableMod(string modName, int position = -1)
  {
    Playset playset = this.LoadActivePlayset();
    if (Enumerable.Any<PlaysetEntry>((IEnumerable<PlaysetEntry>) playset.Mods, (Func<PlaysetEntry, bool>) (m => m.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase))))
    {
      Console.WriteLine($"[PlaysetManager] Mod '{modName}' is already enabled");
    }
    else
    {
      if (position == -1 || position >= playset.Mods.Count)
        position = playset.Mods.Count;
      playset.Mods.Insert(position, new PlaysetEntry()
      {
        ModName = modName,
        LoadOrder = position
      });
      for (int index = 0; index < playset.Mods.Count; ++index)
        playset.Mods[index].LoadOrder = index;
      this.SavePlayset(playset);
      Console.WriteLine($"[PlaysetManager] Enabled mod '{modName}' at position {position}");
    }
  }

  public void DisableMod(string modName)
  {
    Playset playset = this.LoadActivePlayset();
    if (playset.Mods.RemoveAll(m => m.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase)) > 0)
    {
      for (int index = 0; index < playset.Mods.Count; ++index)
        playset.Mods[index].LoadOrder = index;
      this.SavePlayset(playset);
      Console.WriteLine($"[PlaysetManager] Disabled mod '{modName}'");
    }
    else
    {
      Console.WriteLine($"[PlaysetManager] Mod '{modName}' was not in playset");
    }
  }

  public void ReorderMod(string modName, int newPosition)
  {
    Playset playset = this.LoadActivePlayset();
    PlaysetEntry playsetEntry = Enumerable.FirstOrDefault<PlaysetEntry>((IEnumerable<PlaysetEntry>) playset.Mods, (Func<PlaysetEntry, bool>) (m => m.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase)));
    if (playsetEntry == null)
    {
      Console.WriteLine($"[PlaysetManager] Mod '{modName}' not found in playset");
    }
    else
    {
      playset.Mods.Remove(playsetEntry);
      if (newPosition < 0)
        newPosition = 0;
      if (newPosition >= playset.Mods.Count)
        newPosition = playset.Mods.Count;
      playset.Mods.Insert(newPosition, playsetEntry);
      for (int index = 0; index < playset.Mods.Count; ++index)
        playset.Mods[index].LoadOrder = index;
      this.SavePlayset(playset);
      Console.WriteLine($"[PlaysetManager] Moved mod '{modName}' to position {newPosition}");
    }
  }

  public void MoveModUp(string modName)
  {
    PlaysetEntry playsetEntry = Enumerable.FirstOrDefault<PlaysetEntry>((IEnumerable<PlaysetEntry>) this.LoadActivePlayset().Mods, (Func<PlaysetEntry, bool>) (m => m.ModName.Equals(modName, (StringComparison) 5)));
    if (playsetEntry == null || playsetEntry.LoadOrder <= 0)
      return;
    this.ReorderMod(modName, playsetEntry.LoadOrder - 1);
  }

  public void MoveModDown(string modName)
  {
    Playset playset = this.LoadActivePlayset();
    PlaysetEntry playsetEntry = Enumerable.FirstOrDefault<PlaysetEntry>((IEnumerable<PlaysetEntry>) playset.Mods, (Func<PlaysetEntry, bool>) (m => m.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase)));
    if (playsetEntry == null || playsetEntry.LoadOrder >= playset.Mods.Count - 1)
      return;
    this.ReorderMod(modName, playsetEntry.LoadOrder + 1);
  }

  public bool IsModEnabled(string modName)
  {
    return Enumerable.Any<PlaysetEntry>((IEnumerable<PlaysetEntry>) this.LoadActivePlayset().Mods, (Func<PlaysetEntry, bool>) (m => m.ModName.Equals(modName, StringComparison.OrdinalIgnoreCase)));
  }

  public int GetModLoadOrder(string modName)
  {
    PlaysetEntry playsetEntry = Enumerable.FirstOrDefault<PlaysetEntry>((IEnumerable<PlaysetEntry>) this.LoadActivePlayset().Mods, (Func<PlaysetEntry, bool>) (m => m.ModName.Equals(modName, (StringComparison) 5)));
    return playsetEntry == null ? -1 : playsetEntry.LoadOrder;
  }
}
