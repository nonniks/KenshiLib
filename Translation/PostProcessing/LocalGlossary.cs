using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable
namespace KenshiLib.Translation.PostProcessing;

public class LocalGlossary
{
  public string SourceLang { get; set; } = "en";

  public string TargetLang { get; set; } = "ru";

  public Dictionary<string, GlossaryEntry> Entries { get; } = new Dictionary<string, GlossaryEntry>();

  public int Count => this.Entries.Count;

  public void AddEntry(string source, string target, bool inflectable = true)
  {
    this.Entries[source] = new GlossaryEntry()
    {
      SourceTerm = source,
      TargetTerm = target,
      Inflectable = inflectable
    };
  }

  public bool Contains(string source) => this.Entries.ContainsKey(source);

  public void Clear() => this.Entries.Clear();

  public bool IsMorphologySupported() => (this.TargetLang == "ru");

  public string GetMorphologyStatusMessage()
  {
    return "Morphology analysis is only available for Russian (ru) target language.\nCurrent target: " + this.TargetLang;
  }

  public static LocalGlossary Load(string filePath)
  {
    LocalGlossary localGlossary = new LocalGlossary();
    if (!File.Exists(filePath))
      return localGlossary;
    try
    {
      foreach (string readAllLine in File.ReadAllLines(filePath))
      {
        if (!string.IsNullOrWhiteSpace(readAllLine) && !readAllLine.StartsWith("#"))
        {
          string[] strArray = readAllLine.Split('\t', (StringSplitOptions) 0);
          if (strArray.Length >= 2)
          {
            string source = strArray[0];
            string target = strArray[1];
            bool inflectable = strArray.Length <= 2 || (strArray[2] == "1");
            localGlossary.AddEntry(source, target, inflectable);
          }
        }
      }
    }
    catch
    {
    }
    return localGlossary;
  }

  public void Save(string filePath)
  {
    IEnumerable<string> strings = Enumerable.Select<GlossaryEntry, string>((IEnumerable<GlossaryEntry>) this.Entries.Values, (Func<GlossaryEntry, string>) (e => $"{e.SourceTerm}\t{e.TargetTerm}\t{(e.Inflectable ? "1" : "0")}"));
    File.WriteAllLines(filePath, strings);
  }
}
