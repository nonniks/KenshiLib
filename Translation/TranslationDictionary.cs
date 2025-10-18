using KenshiLib.Core;
using KenshiLib.Translation.PostProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace KenshiLib.Translation;

public static class TranslationDictionary
{
  private const string Separator = "|_SEP_|";
  private const string EndMarker = "|_END_|";

  public static double GetTranslationProgress(string dictFile)
  {
    if (!File.Exists(dictFile))
      return 0.0;
    try
    {
      string[] array = Enumerable.ToArray<string>(Enumerable.Where<string>(Enumerable.Select<string, string>((IEnumerable<string>) File.ReadAllText(dictFile).Split(new string[1]
      {
        "|_END_|"
      }, (StringSplitOptions) 1), (Func<string, string>) (s => s.Trim(new char[4]
      {
        '\r',
        '\n',
        ' ',
        '\t'
      }))), (Func<string, bool>) (s => !string.IsNullOrWhiteSpace(s))));
      if (array.Length == 0)
        return 0.0;
      int num1 = 0;
      int num2 = 0;
      foreach (string str in array)
      {
        string[] strArray1 = new string[1]{ "|_SEP_|" };
        string[] strArray2 = str.Split(strArray1, (StringSplitOptions) 0);
        if (strArray2.Length >= 2)
        {
          string key = strArray2[0];
          string text = strArray2[1];
          if (!TranslationDictionary.IsTechnicalName(text, key))
          {
            ++num2;
            if (!string.IsNullOrWhiteSpace(text) && (text != key))
              ++num1;
          }
        }
      }
      return num2 > 0 ? (double) num1 / (double) num2 * 100.0 : 0.0;
    }
    catch
    {
      return 0.0;
    }
  }

  public static bool IsTechnicalName(string text, string key)
  {
    double num;
    return string.IsNullOrWhiteSpace(text) || text.StartsWith("/") && text.EndsWith("/") || text.StartsWith("%") && text.EndsWith("%") || text.StartsWith("$") && text.EndsWith("$") || key.Contains("_id") || key.Contains("_ID") || key.Contains("constant") || key.Contains("CONSTANT") || double.TryParse(text.Trim(), out num) || text.Length <= 2;
  }

  public static void ExportToFile(Dictionary<string, string> dictionary, string filePath)
  {
    IEnumerable<string> strings = Enumerable.Select<KeyValuePair<string, string>, string>((IEnumerable<KeyValuePair<string, string>>) dictionary, (Func<KeyValuePair<string, string>, string>) (kvp => $"{kvp.Key}|_SEP_|{kvp.Value}|_END_|"));
    File.WriteAllText(filePath, string.Join(Environment.NewLine, strings));
  }

  public static Dictionary<string, string> ImportFromFile(string filePath)
  {
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    if (!File.Exists(filePath))
      return dictionary;
    try
    {
      string str1 = File.ReadAllText(filePath);
      string[] strArray1 = new string[1]{ "|_END_|" };
      foreach (string str2 in str1.Split(strArray1, (StringSplitOptions) 1))
      {
        char[] chArray = new char[4]
        {
          '\r',
          '\n',
          ' ',
          '\t'
        };
        string str3 = str2.Trim(chArray);
        if (!string.IsNullOrWhiteSpace(str3))
        {
          string[] strArray2 = str3.Split(new string[1]
          {
            "|_SEP_|"
          }, (StringSplitOptions) 0);
          if (strArray2.Length >= 2)
            dictionary[strArray2[0]] = strArray2[1];
        }
      }
    }
    catch
    {
    }
    return dictionary;
  }

  public static int getTotalToBeTranslated(string dictFilePath)
  {
    if (!File.Exists(dictFilePath))
      return 0;
    try
    {
      string[] strArray1 = File.ReadAllText(dictFilePath).Split(new string[1]
      {
        "|_END_|"
      }, (StringSplitOptions) 1);
      int totalToBeTranslated = 0;
      foreach (string str1 in strArray1)
      {
        char[] chArray = new char[4]
        {
          '\r',
          '\n',
          ' ',
          '\t'
        };
        string str2 = str1.Trim(chArray);
        if (!string.IsNullOrWhiteSpace(str2))
        {
          string[] strArray2 = str2.Split(new string[1]
          {
            "|_SEP_|"
          }, (StringSplitOptions) 0);
          if (strArray2.Length >= 2)
          {
            string key = strArray2[0];
            string text = strArray2[1];
            if (!TranslationDictionary.IsTechnicalName(text, key) && (string.IsNullOrWhiteSpace(text) || (text == key)))
              ++totalToBeTranslated;
          }
        }
      }
      return totalToBeTranslated;
    }
    catch
    {
      return 0;
    }
  }

  public static void ExportToDictFileWithMerge(ReverseEngineer reverseEngineer, string dictFilePath)
  {
    Dictionary<string, string> dictionary1 = File.Exists(dictFilePath) ? TranslationDictionary.ImportFromFile(dictFilePath) : new Dictionary<string, string>();
    Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
    if (reverseEngineer.modData.Records == null)
      return;
    foreach (ModRecord record in reverseEngineer.modData.Records)
    {
      foreach (KeyValuePair<string, string> stringField in record.StringFields)
      {
        string key = $"{record.Id}_{stringField.Key}";
        string text = stringField.Value ?? "";
        if (!TranslationDictionary.IsTechnicalName(text, key))
          dictionary2[key] = dictionary1.ContainsKey(key) ? dictionary1[key] : text;
      }
      foreach (KeyValuePair<string, string> filenameField in record.FilenameFields)
      {
        string key = $"{record.Id}_{filenameField.Key}_filename";
        string text = filenameField.Value ?? "";
        if (!TranslationDictionary.IsTechnicalName(text, key))
          dictionary2[key] = dictionary1.ContainsKey(key) ? dictionary1[key] : text;
      }
    }
    TranslationDictionary.ExportToFile(dictionary2, dictFilePath);
  }

  public static void ImportFromDictFile(ReverseEngineer reverseEngineer, string dictFilePath)
  {
    Dictionary<string, string> dictionary = TranslationDictionary.ImportFromFile(dictFilePath);
    if (reverseEngineer.modData.Records == null)
      return;
    foreach (ModRecord record in reverseEngineer.modData.Records)
    {
      foreach (string str1 in Enumerable.ToList<string>((IEnumerable<string>) record.StringFields.Keys))
      {
        string str2 = $"{record.Id}_{str1}";
        if (dictionary.ContainsKey(str2))
          record.StringFields[str1] = dictionary[str2];
      }
      foreach (string str3 in Enumerable.ToList<string>((IEnumerable<string>) record.FilenameFields.Keys))
      {
        string str4 = $"{record.Id}_{str3}_filename";
        if (dictionary.ContainsKey(str4))
          record.FilenameFields[str3] = dictionary[str4];
      }
    }
  }

  public static async Task ApplyTranslationsAsync(
    string dictFilePath,
    Func<string, Task<string>> translateFunc,
    int delayMs,
    Action<string, string, bool> onTranslationComplete,
    Action<string, string> onTranslationError,
    Func<List<string>, Task<List<string>>>? batchTranslateFunc = null,
    MorphologyPostProcessor? morphologyProcessor = null,
    LocalGlossary? localGlossary = null,
    Action<int, int>? onProgressUpdate = null)
  {
    Dictionary<string, string> dict = TranslationDictionary.ImportFromFile(dictFilePath);
    List<KeyValuePair<string, string>> list = Enumerable.ToList<KeyValuePair<string, string>>(Enumerable.Where<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>) dict, (Func<KeyValuePair<string, string>, bool>) (kvp => string.IsNullOrWhiteSpace(kvp.Value) || (kvp.Value == kvp.Key))));
    int done = 0;
    int total = list.Count;
    if (batchTranslateFunc != null && delayMs == 100)
    {
      List<string> texts = Enumerable.ToList<string>(Enumerable.Select<KeyValuePair<string, string>, string>((IEnumerable<KeyValuePair<string, string>>) list, (Func<KeyValuePair<string, string>, string>) (kvp => kvp.Key)));
      try
      {
        List<string> stringList = await batchTranslateFunc(texts);
        for (int index = 0; index < texts.Count; ++index)
        {
          if (index < stringList.Count)
          {
            string str = texts[index];
            string translatedText = stringList[index];
            if (morphologyProcessor != null)
              translatedText = morphologyProcessor.Process(translatedText, "ru");
            dict[str] = translatedText;
            Action<string, string, bool> action1 = onTranslationComplete;
            if (action1 != null)
              action1(str, translatedText, true);
            ++done;
            Action<int, int> action2 = onProgressUpdate;
            if (action2 != null)
              action2(done, total);
          }
          else
            break;
        }
      }
      catch (Exception ex)
      {
        Action<string, string> action = onTranslationError;
        if (action != null)
          action("batch", ex.Message);
      }
      texts = (List<string>) null;
    }
    else
    {
      foreach (KeyValuePair<string, string> keyValuePair in list)
      {
        KeyValuePair<string, string> kvp = keyValuePair;
        try
        {
          string translatedText = await translateFunc(kvp.Key);
          if (morphologyProcessor != null)
            translatedText = morphologyProcessor.Process(translatedText, "ru");
          dict[kvp.Key] = translatedText;
          Action<string, string, bool> action = onTranslationComplete;
          if (action != null)
            action(kvp.Key, translatedText, true);
        }
        catch (Exception ex)
        {
          Action<string, string> action = onTranslationError;
          if (action != null)
            action(kvp.Key, ex.Message);
        }
        ++done;
        Action<int, int> action3 = onProgressUpdate;
        if (action3 != null)
          action3(done, total);
        await Task.Delay(delayMs);
        kvp = new KeyValuePair<string, string>();
      }
    }
    TranslationDictionary.ExportToFile(dict, dictFilePath);
    dict = (Dictionary<string, string>) null;
  }

  public static CleanResult CleanDictionary(string dictFilePath)
  {
    try
    {
      Dictionary<string, string> dictionary = TranslationDictionary.ImportFromFile(dictFilePath);
      int num1 = 0;
      int num2 = 0;
      List<string> stringList = new List<string>();
      foreach (KeyValuePair<string, string> keyValuePair in Enumerable.ToList<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>) dictionary))
      {
        string key = keyValuePair.Key;
        string text = keyValuePair.Value;
        if (key.Contains("/") && !text.Contains("/"))
        {
          dictionary[key] = key;
          ++num1;
        }
        if (TranslationDictionary.IsTechnicalName(text, key) && string.IsNullOrWhiteSpace(text))
        {
          stringList.Add(key);
          ++num2;
        }
      }
      foreach (string str in stringList)
        dictionary.Remove(str);
      TranslationDictionary.ExportToFile(dictionary, dictFilePath);
      return new CleanResult()
      {
        Success = true,
        CleanedEntries = num1,
        RemovedEntries = num2
      };
    }
    catch (Exception ex)
    {
      return new CleanResult()
      {
        Success = false,
        ErrorMessage = ex.Message
      };
    }
  }

  public static CleanResult CleanDictionaryVariablesOnly(string dictFilePath)
  {
    try
    {
      Dictionary<string, string> dictionary = TranslationDictionary.ImportFromFile(dictFilePath);
      int num = 0;
      foreach (KeyValuePair<string, string> keyValuePair in Enumerable.ToList<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>) dictionary))
      {
        string key = keyValuePair.Key;
        string str = keyValuePair.Value;
        if ((key.Contains("/") || key.Contains("%") || key.Contains("$")) && !str.Contains("/") && !str.Contains("%") && !str.Contains("$"))
        {
          dictionary[key] = key;
          ++num;
        }
      }
      TranslationDictionary.ExportToFile(dictionary, dictFilePath);
      return new CleanResult()
      {
        Success = true,
        CleanedEntries = num,
        RemovedEntries = 0
      };
    }
    catch (Exception ex)
    {
      return new CleanResult()
      {
        Success = false,
        ErrorMessage = ex.Message
      };
    }
  }
}
