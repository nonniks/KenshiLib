using KenshiLib.Translation.PostProcessing;

#nullable enable
namespace KenshiLib.Translation;

public static class GlossaryManager
{
  public static string ApplyLocalGlossary(
    string translatedText,
    string sourceLang,
    string targetLang)
  {
    return GlossaryHelper.ApplyLocalGlossary(translatedText, sourceLang, targetLang);
  }

  public static void LoadGlossary(string filePath, string sourceLang, string targetLang)
  {
    GlossaryHelper.LoadGlossaryFromFile(filePath, sourceLang, targetLang);
  }

  public static void AddEntry(
    string sourceLang,
    string targetLang,
    string sourceTerm,
    string targetTerm)
  {
    GlossaryHelper.AddEntry(sourceLang, targetLang, sourceTerm, targetTerm);
  }

  public static void Clear() => GlossaryHelper.Clear();
}
