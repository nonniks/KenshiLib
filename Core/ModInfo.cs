using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

#nullable enable
namespace KenshiLib.Core;

public class ModInfo : INotifyPropertyChanged
{
  private bool _isEnabled;
  private int _loadOrder = -1;

  public event PropertyChangedEventHandler? PropertyChanged;

  protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
  {
    if (EqualityComparer<T>.Default.Equals(field, value))
      return false;

    field = value;
    OnPropertyChanged(propertyName);
    return true;
  }

  public string Name { get; set; } = string.Empty;

  public string? FilePath { get; set; }

  public long FileSize { get; set; }

  public DateTime LastModified { get; set; }

  public bool InGameDir { get; set; }

  public bool InWorkshop { get; set; }

  public long WorkshopId { get; set; } = -1;

  public bool IsMarkedSelected { get; set; }

  public bool IsEnabled
  {
    get => _isEnabled;
    set => SetProperty(ref _isEnabled, value);
  }

  public int LoadOrder
  {
    get => _loadOrder;
    set => SetProperty(ref _loadOrder, value);
  }

  public string? ImagePath { get; set; }

  public string DetectedLanguage { get; set; } = "detecting...";

  public int TranslationProgress { get; set; }

  public int StringCount { get; set; }

  public bool HasDictionary { get; set; }

  public bool HasBackup { get; set; }

  public ValidationStatus ValidationStatus { get; set; }

  public int IssuesCount { get; set; }

  public int CriticalIssuesCount { get; set; }

  public int ErrorIssuesCount { get; set; }

  public int WarningIssuesCount { get; set; }

  public string Author { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string Dependencies { get; set; } = string.Empty;

  public string References { get; set; } = string.Empty;

  public int ModVersion { get; set; }

  public int FileType { get; set; }

  public int RecordCount { get; set; }

  public bool IsSelected { get; set; }

  public bool IsExpanded { get; set; }

  public string Location
  {
    get
    {
      List<string> stringList = new List<string>();
      if (this.InGameDir)
        stringList.Add("Game");
      if (this.InWorkshop)
        stringList.Add("Workshop");
      if (this.IsMarkedSelected)
        stringList.Add("Selected");
      return stringList.Count <= 0 ? "Unknown" : string.Join(" + ", (IEnumerable<string>) stringList);
    }
  }

  public string TranslationProgressText => $"{this.TranslationProgress}%";

  public string ValidationStatusIcon
  {
    get
    {
      string validationStatusIcon;
      switch (this.ValidationStatus)
      {
        case ValidationStatus.Validating:
          validationStatusIcon = "⟳";
          break;
        case ValidationStatus.OK:
          validationStatusIcon = "✓";
          break;
        case ValidationStatus.Warning:
          validationStatusIcon = "⚠";
          break;
        case ValidationStatus.Error:
          validationStatusIcon = "✗";
          break;
        case ValidationStatus.Critical:
          validationStatusIcon = "\uD83D\uDD34";
          break;
        default:
          validationStatusIcon = "○";
          break;
      }
      return validationStatusIcon;
    }
  }

  public string? GetDictFilePath()
  {
    return string.IsNullOrEmpty(this.FilePath) ? (string) null : Path.Combine(Path.GetDirectoryName(this.FilePath), Path.GetFileNameWithoutExtension(this.Name) + ".dict");
  }

  public string? GetBackupFilePath()
  {
    return string.IsNullOrEmpty(this.FilePath) ? (string) null : Path.Combine(Path.GetDirectoryName(this.FilePath), Path.GetFileNameWithoutExtension(this.Name) + ".backup");
  }

  public string? GetTechDictFilePath()
  {
    return string.IsNullOrEmpty(this.FilePath) ? (string) null : Path.Combine(Path.GetDirectoryName(this.FilePath), Path.GetFileNameWithoutExtension(this.Name) + ".tech.dict");
  }
}
