using GTranslate.Translators;
using KenshiLib.Translator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace KenshiLib.Translation.Providers;

public class GTranslate_Translator : TranslatorInterface
{
  private static GTranslate_Translator? _instance;
  private ITranslator _translator;
  private GTranslate_Translator.ProviderType _provider;

  public static GTranslate_Translator Instance
  {
    get
    {
      return GTranslate_Translator._instance ?? (GTranslate_Translator._instance = new GTranslate_Translator());
    }
  }

  public GTranslate_Translator(GTranslate_Translator.ProviderType provider = GTranslate_Translator.ProviderType.Google)
  {
    this._provider = provider;
    ITranslator translator;
    switch (provider)
    {
      case GTranslate_Translator.ProviderType.Google:
        translator = (ITranslator) new GoogleTranslator();
        break;
      case GTranslate_Translator.ProviderType.Bing:
        translator = (ITranslator) new BingTranslator();
        break;
      case GTranslate_Translator.ProviderType.Yandex:
        translator = (ITranslator) new YandexTranslator();
        break;
      case GTranslate_Translator.ProviderType.MicrosoftTranslator:
        translator = (ITranslator) new MicrosoftTranslator();
        break;
      default:
        translator = (ITranslator) new GoogleTranslator();
        break;
    }
    this._translator = translator;
    Console.WriteLine($"[GTranslate_Translator] Initialized with provider: {this._provider}");
  }

  public async Task<string> TranslateAsync(string text, string from = "auto", string to = "en")
  {
    if (string.IsNullOrWhiteSpace(text))
      return text;
    try
    {
      return (await this._translator.TranslateAsync(text, to, from))?.Translation ?? text;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"[GTranslate_Translator] Translation failed with {this._provider}: {ex.Message}");
      throw new Exception($"GTranslate {this._provider} failed: {ex.Message}", ex);
    }
  }

  public async Task<Dictionary<string, string>> GetSupportedLanguagesAsync()
  {
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    dictionary.Add("auto", "Auto Detect");
    dictionary.Add("en", "English");
    dictionary.Add("ru", "Russian");
    dictionary.Add("es", "Spanish");
    dictionary.Add("fr", "French");
    dictionary.Add("de", "German");
    dictionary.Add("it", "Italian");
    dictionary.Add("ja", "Japanese");
    dictionary.Add("ko", "Korean");
    dictionary.Add("zh", "Chinese (Simplified)");
    dictionary.Add("pt", "Portuguese");
    dictionary.Add("ar", "Arabic");
    dictionary.Add("hi", "Hindi");
    dictionary.Add("tr", "Turkish");
    dictionary.Add("pl", "Polish");
    dictionary.Add("uk", "Ukrainian");
    dictionary.Add("cs", "Czech");
    dictionary.Add("nl", "Dutch");
    dictionary.Add("sv", "Swedish");
    dictionary.Add("no", "Norwegian");
    return await Task.FromResult<Dictionary<string, string>>(dictionary);
  }

  public static async Task<GTranslate_Translator> CreateWithBestProviderAsync()
  {
    GTranslate_Translator.ProviderType[] providerTypeArray = new GTranslate_Translator.ProviderType[4]
    {
      GTranslate_Translator.ProviderType.Google,
      GTranslate_Translator.ProviderType.Bing,
      GTranslate_Translator.ProviderType.MicrosoftTranslator,
      GTranslate_Translator.ProviderType.Yandex
    };
    for (int index = 0; index < providerTypeArray.Length; ++index)
    {
      GTranslate_Translator.ProviderType provider = providerTypeArray[index];
      try
      {
        GTranslate_Translator translator = new GTranslate_Translator(provider);
        string str = await translator.TranslateAsync("test", "en", "en");
        Console.WriteLine($"[GTranslate_Translator] Selected provider: {provider}");
        return translator;
      }
      catch
      {
        Console.WriteLine($"[GTranslate_Translator] Provider {provider} unavailable");
      }
    }
    providerTypeArray = (GTranslate_Translator.ProviderType[]) null;
    Console.WriteLine("[GTranslate_Translator] All providers failed test, falling back to Google");
    return new GTranslate_Translator();
  }

  public void setTranslator(string providerCode)
  {
    string lower = providerCode.ToLower();
    this._provider = (lower == "google") ? GTranslate_Translator.ProviderType.Google : ((lower == "bing") ? GTranslate_Translator.ProviderType.Bing : ((lower == "yandex") ? GTranslate_Translator.ProviderType.Yandex : ((lower == "microsoft") ? GTranslate_Translator.ProviderType.MicrosoftTranslator : GTranslate_Translator.ProviderType.Google)));
    ITranslator translator;
    switch (this._provider)
    {
      case GTranslate_Translator.ProviderType.Google:
        translator = (ITranslator) new GoogleTranslator();
        break;
      case GTranslate_Translator.ProviderType.Bing:
        translator = (ITranslator) new BingTranslator();
        break;
      case GTranslate_Translator.ProviderType.Yandex:
        translator = (ITranslator) new YandexTranslator();
        break;
      case GTranslate_Translator.ProviderType.MicrosoftTranslator:
        translator = (ITranslator) new MicrosoftTranslator();
        break;
      default:
        translator = (ITranslator) new GoogleTranslator();
        break;
    }
    this._translator = translator;
    Console.WriteLine($"[GTranslate_Translator] Switched to provider: {this._provider}");
  }

  public enum ProviderType
  {
    Google,
    Bing,
    Yandex,
    MicrosoftTranslator,
  }
}
