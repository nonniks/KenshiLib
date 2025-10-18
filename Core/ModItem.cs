using System;
using System.IO;

#nullable enable
namespace KenshiLib.Core;

public class ModItem
{
  public string Name { get; set; }

  public string Language { get; set; } = "detecting...";

  public bool InGameDir { get; set; }

  public bool Selected { get; set; }

  public long WorkshopId { get; set; }

  public ModItem(string name)
  {
    this.InGameDir = false;
    this.Selected = false;
    this.WorkshopId = -1L;
    this.Name = name ?? throw new ArgumentNullException(nameof (name));
  }

  public string? getBackupFilePath()
  {
    string modFilePath = this.getModFilePath();
    return string.IsNullOrEmpty(modFilePath) ? (string) null : Path.Combine(Path.GetDirectoryName(modFilePath), Path.GetFileNameWithoutExtension(this.Name) + ".backup");
  }

  public string? getDictFilePath()
  {
    string modFilePath = this.getModFilePath();
    return string.IsNullOrEmpty(modFilePath) ? (string) null : Path.Combine(Path.GetDirectoryName(modFilePath), Path.GetFileNameWithoutExtension(this.Name) + ".dict");
  }

  public string? getModFilePath()
  {
    if (this.InGameDir)
      return Path.Combine(ModManager.gamedirModsPath, Path.GetFileNameWithoutExtension(this.Name), this.Name);
    return this.WorkshopId != -1L ? Path.Combine(ModManager.workshopModsPath, this.WorkshopId.ToString(), this.Name) : (string) null;
  }
}
