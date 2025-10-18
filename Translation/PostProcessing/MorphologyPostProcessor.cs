using DeepMorphy;
using DeepMorphy.Model;
using System;
using System.Collections.Generic;

#nullable enable
namespace KenshiLib.Translation.PostProcessing;

public class MorphologyPostProcessor
{
  private readonly MorphAnalyzer? _analyzer;
  private readonly bool _isInitialized;

  public MorphologyPostProcessor()
  {
    try
    {
      this._analyzer = new MorphAnalyzer(true);
      this._isInitialized = true;
      Console.WriteLine("[MorphologyPostProcessor] DeepMorphy initialized successfully");
    }
    catch (Exception ex)
    {
      Console.WriteLine("[MorphologyPostProcessor] Failed to initialize DeepMorphy: " + ex.Message);
      this._isInitialized = false;
    }
  }

  public string Process(string translatedText, string targetLang)
  {
    return (targetLang.ToLower() != "ru") || !this._isInitialized || this._analyzer == null || !string.IsNullOrWhiteSpace(translatedText) ? translatedText : translatedText;
  }

  public List<string> AnalyzeWord(string word)
  {
    if (this._isInitialized)
    {
      if (this._analyzer != null)
      {
        try
        {
          IEnumerable<MorphInfo> morphInfos = this._analyzer.Parse(new string[1]
          {
            word
          });
          List<string> stringList = new List<string>();
          foreach (MorphInfo morphInfo in morphInfos)
          {
            if (morphInfo.BestTag != null)
              stringList.Add($"Tag: {morphInfo.BestTag}");
          }
          return stringList;
        }
        catch
        {
          return new List<string>();
        }
      }
    }
    return new List<string>();
  }

  public string? GetLemma(string word)
  {
    if (this._isInitialized)
    {
      if (this._analyzer != null)
      {
        try
        {
          foreach (MorphInfo morphInfo in this._analyzer.Parse(new string[1]
          {
            word
          }))
          {
            if (morphInfo.BestTag != null && !string.IsNullOrEmpty(morphInfo.BestTag.Lemma))
              return morphInfo.BestTag.Lemma;
          }
        }
        catch
        {
        }
        return (string) null;
      }
    }
    return (string) null;
  }

  public bool IsInitialized => this._isInitialized;

  public bool IsMorphologyAvailable => this._isInitialized;

  public List<(string Form, string Description)>? GenerateAllForms(string word)
  {
    if (this._isInitialized)
    {
      if (this._analyzer != null)
      {
        try
        {
          IEnumerable<MorphInfo> morphInfos = this._analyzer.Parse(new string[1]
          {
            word
          });
          List<(string, string)> valueTupleList = new List<(string, string)>();
          foreach (MorphInfo morphInfo in morphInfos)
          {
            if (morphInfo.BestTag != null)
              valueTupleList.Add((morphInfo.Text, ((object) morphInfo.BestTag).ToString() ?? "Unknown"));
          }
          return valueTupleList.Count > 0 ? valueTupleList : (List<(string, string)>) null;
        }
        catch
        {
          return (List<(string, string)>) null;
        }
      }
    }
    return (List<(string, string)>) null;
  }
}
