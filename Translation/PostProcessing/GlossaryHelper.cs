using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

#nullable enable
namespace KenshiLib.Translation.PostProcessing;

public static class GlossaryHelper
{
  private static Dictionary<string, Dictionary<string, string>> _glossaries = new Dictionary<string, Dictionary<string, string>>();
  private static readonly object _lock = new object();

  static GlossaryHelper() => GlossaryHelper.InitializeDefaultGlossaries();

  public static string ApplyLocalGlossary(
    string translatedText,
    string sourceLang,
    string targetLang)
  {
    if (string.IsNullOrWhiteSpace(translatedText))
      return translatedText;
    string lower = $"{sourceLang}_{targetLang}".ToLower();
    lock (GlossaryHelper._lock)
    {
      if (!GlossaryHelper._glossaries.ContainsKey(lower))
        return translatedText;
      Dictionary<string, string> glossary = GlossaryHelper._glossaries[lower];
      string str1 = translatedText;
      foreach (KeyValuePair<string, string> keyValuePair in glossary)
      {
        string str2 = $"\\b{Regex.Escape(keyValuePair.Key)}\\b";
        str1 = Regex.Replace(str1, str2, keyValuePair.Value, (RegexOptions) 1);
      }
      return str1;
    }
  }

  public static void LoadGlossaryFromFile(string filePath, string sourceLang, string targetLang)
  {
    if (!File.Exists(filePath))
      return;
    string lower = $"{sourceLang}_{targetLang}".ToLower();
    Dictionary<string, string> dictionary = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    try
    {
      foreach (string readAllLine in File.ReadAllLines(filePath))
      {
        if (!string.IsNullOrWhiteSpace(readAllLine) && !readAllLine.StartsWith("#"))
        {
          string[] strArray = readAllLine.Split(',', (StringSplitOptions) 0);
          if (strArray.Length >= 2)
          {
            string str1 = strArray[0].Trim();
            string str2 = strArray[1].Trim();
            dictionary[str1] = str2;
          }
        }
      }
      lock (GlossaryHelper._lock)
        GlossaryHelper._glossaries[lower] = dictionary;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[LocalGlossary] Error loading glossary from {filePath}: {ex.Message}");
    }
  }

  public static void AddEntry(
    string sourceLang,
    string targetLang,
    string sourceTerm,
    string targetTerm)
  {
    string lower = $"{sourceLang}_{targetLang}".ToLower();
    lock (GlossaryHelper._lock)
    {
      if (!GlossaryHelper._glossaries.ContainsKey(lower))
        GlossaryHelper._glossaries[lower] = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
      GlossaryHelper._glossaries[lower][sourceTerm] = targetTerm;
    }
  }

  public static void Clear()
  {
    lock (GlossaryHelper._lock)
    {
      GlossaryHelper._glossaries.Clear();
      GlossaryHelper.InitializeDefaultGlossaries();
    }
  }

  private static void InitializeDefaultGlossaries()
  {
    Dictionary<string, string> dictionary1 = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    dictionary1.Add("mod", "мод");
    dictionary1.Add("playset", "плейсет");
    dictionary1.Add("load order", "порядок загрузки");
    dictionary1.Add("workshop", "мастерская");
    dictionary1.Add("skeleton", "скелет");
    dictionary1.Add("hive", "улей");
    dictionary1.Add("shek", "шек");
    dictionary1.Add("holy nation", "священная нация");
    dictionary1.Add("tech hunters", "охотники за технологиями");
    dictionary1.Add("cannibals", "каннибалы");
    dictionary1.Add("fogmen", "туманники");
    dictionary1.Add("strength", "сила");
    dictionary1.Add("toughness", "выносливость");
    dictionary1.Add("dexterity", "ловкость");
    dictionary1.Add("perception", "восприятие");
    dictionary1.Add("katana", "катана");
    dictionary1.Add("crossbow", "арбалет");
    dictionary1.Add("medkit", "аптечка");
    dictionary1.Add("bandage", "бинт");
    Dictionary<string, string> dictionary2 = dictionary1;
    GlossaryHelper._glossaries["en_ru"] = dictionary2;
    GlossaryHelper._glossaries["auto_ru"] = dictionary2;
  }

  public static Dictionary<string, string> GetGlossary(string sourceLang, string targetLang)
  {
    string lower = $"{sourceLang}_{targetLang}".ToLower();
    lock (GlossaryHelper._lock)
    {
      if (GlossaryHelper._glossaries.ContainsKey(lower))
        return new Dictionary<string, string>((IDictionary<string, string>) GlossaryHelper._glossaries[lower]);
    }
    return new Dictionary<string, string>();
  }
}
