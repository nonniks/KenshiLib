using System.Collections.Generic;

#nullable enable
namespace KenshiLib.Core;

public class ModInstance
{
  public string? Id { get; set; }

  public string? Target { get; set; }

  public float Tx { get; set; }

  public float Ty { get; set; }

  public float Tz { get; set; }

  public float Rw { get; set; }

  public float Rx { get; set; }

  public float Ry { get; set; }

  public float Rz { get; set; }

  public int StateCount { get; set; }

  public List<string>? States { get; set; }
}
