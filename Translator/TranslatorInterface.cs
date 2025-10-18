using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace KenshiLib.Translator;

public interface TranslatorInterface
{
  Task<string> TranslateAsync(string text, string from = "auto", string to = "en");

  Task<Dictionary<string, string>> GetSupportedLanguagesAsync();
}
