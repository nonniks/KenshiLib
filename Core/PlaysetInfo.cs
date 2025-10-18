using System;
using System.IO;

#nullable enable
namespace KenshiLib.Core;

public class PlaysetInfo
{
  public string Name { get; set; } = string.Empty;

  public string FilePath { get; set; } = string.Empty;

  public int ModCount { get; set; }

  public DateTime CreatedDate { get; set; }

  public DateTime ModifiedDate { get; set; }

  public bool IsActive { get; set; }

  public string FileName => Path.GetFileNameWithoutExtension(this.FilePath);

  public virtual string ToString() => this.Name;
}
