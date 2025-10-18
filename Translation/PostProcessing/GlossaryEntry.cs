#nullable enable
namespace KenshiLib.Translation.PostProcessing;

public class GlossaryEntry
{
  public string SourceTerm { get; set; } = string.Empty;

  public string TargetTerm { get; set; } = string.Empty;

  public bool Inflectable { get; set; } = true;
}
