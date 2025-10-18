using System;

#nullable enable
namespace KenshiLib.Core;

public class MovedModRecord
{
  public string ModName { get; set; } = "";

  public long WorkshopId { get; set; }

  public string GameDirPath { get; set; } = "";

  public DateTime MovedDate { get; set; }

  public string WorkshopPath { get; set; } = "";
}
