#nullable enable
namespace KenshiLib.Core;

public class PlaysetModEntry
{
  public string ModName { get; set; } = string.Empty;

  public bool IsEnabled { get; set; } = true;

  public string OriginalLine { get; set; } = string.Empty;
}
