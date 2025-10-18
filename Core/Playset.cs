using System.Collections.Generic;

#nullable enable
namespace KenshiLib.Core;

public class Playset
{
  public string Name { get; set; } = "Unnamed Playset";

  public List<PlaysetEntry> Mods { get; set; } = new List<PlaysetEntry>();
}
