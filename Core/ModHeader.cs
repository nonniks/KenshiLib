#nullable enable
namespace KenshiLib.Core;

public class ModHeader
{
  public int FileType { get; set; }

  public int ModVersion { get; set; }

  public string Author { get; set; } = "";

  public string Description { get; set; } = "";

  public string Dependencies { get; set; } = "";

  public string References { get; set; } = "";

  public int UnknownInt { get; set; }

  public int RecordCount { get; set; }

  public int DetailsLength { get; set; }

  public byte[]? Details { get; set; }
}
