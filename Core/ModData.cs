using System.Collections.Generic;

#nullable enable
namespace KenshiLib.Core;

public class ModData
{
  public ModHeader? Header { get; set; }

  public List<ModRecord>? Records { get; set; }

  public byte[]? Leftover { get; set; }
}
