#nullable enable
namespace KenshiLib.Translation;

public class CleanResult
{
  public bool Success { get; set; }

  public int CleanedEntries { get; set; }

  public int RemovedEntries { get; set; }

  public string? ErrorMessage { get; set; }
}
