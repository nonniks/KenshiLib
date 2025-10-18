using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;
using Google.LongRunning;
using Grpc.Core;
using KenshiLib.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable
namespace KenshiLib.Translator;

public class CustomApiTranslator : TranslatorInterface
{
  private readonly string _apiKey;
  private readonly HttpClient _httpClient;
  private readonly ApiType _apiType;
  private TranslationServiceClient? _googleV3Client;
  private string? _projectId;
  private readonly string _glossaryName = "glossary_kenshi_en_ru";
  private readonly string _bucketUri = "gs://kenshi-glossary/glossary_kenshi_en_ru.csv";
  private readonly string _expectedProjectId = "handy-sensor-472801-b1";

  public string Name => "Custom API";

  public ApiType CurrentApiType => this._apiType;

  public CustomApiTranslator(string apiInput)
  {
    this._apiKey = !string.IsNullOrWhiteSpace(apiInput) ? apiInput : throw new ArgumentNullException(nameof (apiInput));
    this._httpClient = new HttpClient();
    this._httpClient.Timeout = TimeSpan.FromSeconds(30L);
    this._apiType = this.DetermineApiType(apiInput);
    if (this._apiType == ApiType.DeepL)
    {
      this._httpClient.DefaultRequestHeaders.Add("Authorization", "DeepL-Auth-Key " + this._apiKey);
      this._httpClient.DefaultRequestHeaders.Add("User-Agent", "KenshiTranslator/1.0");
    }
    else
    {
      if (this._apiType != ApiType.GoogleCloudV3)
        return;
      this.InitializeGoogleV3Client();
    }
  }

  private ApiType DetermineApiType(string apiInput)
  {
    if (apiInput.Length == 39 && (apiInput.EndsWith(":fx") || !apiInput.Contains(".")))
      return ApiType.DeepL;
    if (apiInput.StartsWith("AIza") && apiInput.Length >= 35)
      return ApiType.GoogleCloud;
    if (apiInput.EndsWith(".json", (StringComparison) 5) && File.Exists(apiInput))
      return ApiType.GoogleCloudV3;
    return apiInput.StartsWith("http") ? ApiType.Generic : ApiType.DeepL;
  }

  private void InitializeGoogleV3Client()
  {
    try
    {
      Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", this._apiKey);
      this._googleV3Client = TranslationServiceClient.Create();
      this._projectId = this.GetProjectIdFromJson(this._apiKey);
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException("Failed to initialize Google Cloud Translation V3: " + ex.Message, ex);
    }
  }

  public async Task<string> TranslateAsync(string text, string sourceLang = "auto", string targetLang = "en")
  {
    string str1;
    try
    {
      string str2;
      switch (this._apiType)
      {
        case ApiType.DeepL:
          str2 = await this.TranslateWithDeepL(text, sourceLang, targetLang);
          break;
        case ApiType.GoogleCloud:
          str2 = await this.TranslateWithGoogle(text, sourceLang, targetLang);
          break;
        case ApiType.GoogleCloudV3:
          str2 = await this.TranslateWithGoogleV3(text, sourceLang, targetLang);
          break;
        case ApiType.Generic:
          str2 = await this.TranslateWithGenericApi(text, sourceLang, targetLang);
          break;
        default:
          throw new NotSupportedException($"API type {this._apiType} not supported");
      }
      string translatedText = str2;
      if (this._apiType != ApiType.GoogleCloudV3)
      {
        string str3 = GlossaryManager.ApplyLocalGlossary(translatedText, sourceLang, targetLang);
        if ((str3 != translatedText))
          translatedText = str3;
      }
      str1 = translatedText;
    }
    catch (Exception ex)
    {
      throw new Exception("Custom API translation failed: " + ex.Message, ex);
    }
    return str1;
  }

  private async Task<string> TranslateWithDeepL(string text, string sourceLang, string targetLang)
  {
    HttpResponseMessage response = await this._httpClient.PostAsync(this._apiKey.EndsWith(":fx") ? "https://api-free.deepl.com/v2/translate" : "https://api.deepl.com/v2/translate", (HttpContent) new StringContent(JsonSerializer.Serialize(new
    {
      text = new string[1]{ text },
      target_lang = targetLang.ToUpper(),
      source_lang = !(sourceLang != "auto") || string.IsNullOrEmpty(sourceLang) ? (string) null : sourceLang.ToUpper()
    }, new JsonSerializerOptions()
    {
      DefaultIgnoreCondition = (JsonIgnoreCondition) 3
    }), Encoding.UTF8, "application/json"));
    string str1 = await response.Content.ReadAsStringAsync();
    response.EnsureSuccessStatusCode();
    CustomApiTranslator.DeepLResponse deepLresponse = JsonSerializer.Deserialize<CustomApiTranslator.DeepLResponse>(str1, (JsonSerializerOptions) null);
    string str2;
    if (deepLresponse == null)
    {
      str2 = (string) null;
    }
    else
    {
      CustomApiTranslator.DeepLTranslation[] translations = deepLresponse.translations;
      str2 = translations != null ? Enumerable.FirstOrDefault<CustomApiTranslator.DeepLTranslation>((IEnumerable<CustomApiTranslator.DeepLTranslation>) translations)?.text : (string) null;
    }
    if (str2 == null)
      str2 = text;
    string str3 = str2;
    response = (HttpResponseMessage) null;
    return str3;
  }

  private async Task<string> TranslateWithGoogle(string text, string sourceLang, string targetLang)
  {
    return await this.TranslateWithGooglePost(text, sourceLang, targetLang);
  }

  private async Task<string> TranslateWithGooglePost(
    string text,
    string sourceLang,
    string targetLang)
  {
    List<string> stringList = new List<string>();
    stringList.Add("key=" + Uri.EscapeDataString(this._apiKey));
    HttpResponseMessage response = await this._httpClient.PostAsync($"https://translation.googleapis.com/language/translate/v2?{string.Join("&", (IEnumerable<string>) stringList)}", (HttpContent) new StringContent(JsonSerializer.Serialize(new
    {
      q = text,
      target = targetLang.ToLower(),
      source = !(sourceLang != "auto") || string.IsNullOrEmpty(sourceLang) ? (string) null : sourceLang.ToLower(),
      format = nameof (text)
    }, new JsonSerializerOptions()
    {
      DefaultIgnoreCondition = (JsonIgnoreCondition) 3
    }), Encoding.UTF8, "application/json"));
    string str1 = await response.Content.ReadAsStringAsync();
    if (!response.IsSuccessStatusCode)
    {
      if (response.StatusCode == (HttpStatusCode)403 || response.StatusCode == (HttpStatusCode)404)
        return await this.TranslateWithGoogleLegacy(text, sourceLang, targetLang);
      throw new HttpRequestException($"Google Translate API error {response.StatusCode}: {str1}");
    }
    CustomApiTranslator.GoogleResponse googleResponse = JsonSerializer.Deserialize<CustomApiTranslator.GoogleResponse>(str1, (JsonSerializerOptions) null);
    string str2;
    if (googleResponse == null)
    {
      str2 = (string) null;
    }
    else
    {
      CustomApiTranslator.GoogleData data = googleResponse.data;
      if (data == null)
      {
        str2 = (string) null;
      }
      else
      {
        CustomApiTranslator.GoogleTranslation[] translations = data.translations;
        str2 = translations != null ? Enumerable.FirstOrDefault<CustomApiTranslator.GoogleTranslation>((IEnumerable<CustomApiTranslator.GoogleTranslation>) translations)?.translatedText : (string) null;
      }
    }
    if (str2 == null)
      str2 = text;
    return str2;
  }

  private async Task<string> TranslateWithGoogleLegacy(
    string text,
    string sourceLang,
    string targetLang)
  {
    List<string> stringList1 = new List<string>();
    stringList1.Add("key=" + Uri.EscapeDataString(this._apiKey));
    stringList1.Add("q=" + Uri.EscapeDataString(text));
    stringList1.Add("target=" + targetLang.ToLower());
    stringList1.Add("format=text");
    List<string> stringList2 = stringList1;
    if ((sourceLang != "auto") && !string.IsNullOrEmpty(sourceLang))
      stringList2.Add("source=" + sourceLang.ToLower());
    HttpResponseMessage response = await this._httpClient.GetAsync($"https://www.googleapis.com/language/translate/v2?{string.Join("&", (IEnumerable<string>) stringList2)}");
    string str1 = await response.Content.ReadAsStringAsync();
    response.EnsureSuccessStatusCode();
    CustomApiTranslator.GoogleResponse googleResponse = JsonSerializer.Deserialize<CustomApiTranslator.GoogleResponse>(str1, (JsonSerializerOptions) null);
    string str2;
    if (googleResponse == null)
    {
      str2 = (string) null;
    }
    else
    {
      CustomApiTranslator.GoogleData data = googleResponse.data;
      if (data == null)
      {
        str2 = (string) null;
      }
      else
      {
        CustomApiTranslator.GoogleTranslation[] translations = data.translations;
        str2 = translations != null ? Enumerable.FirstOrDefault<CustomApiTranslator.GoogleTranslation>((IEnumerable<CustomApiTranslator.GoogleTranslation>) translations)?.translatedText : (string) null;
      }
    }
    if (str2 == null)
      str2 = text;
    string str3 = str2;
    response = (HttpResponseMessage) null;
    return str3;
  }

  private async Task<string> TranslateWithGoogleV3(
    string text,
    string sourceLang,
    string targetLang)
  {
    if (this._googleV3Client == null || string.IsNullOrEmpty(this._projectId))
      throw new InvalidOperationException("Google V3 client not initialized");
    try
    {
      string str1 = ((object) new LocationName(this._projectId, "global")).ToString();
      TranslateTextRequest translateTextRequest = new TranslateTextRequest()
      {
        Parent = str1,
        TargetLanguageCode = targetLang.ToLower(),
        Contents = {
          text
        }
      };
      if ((sourceLang != "auto") && !string.IsNullOrEmpty(sourceLang))
        translateTextRequest.SourceLanguageCode = sourceLang.ToLower();
      if (this.ShouldUseGlossary(sourceLang, targetLang))
      {
        string str2 = $"projects/{this._projectId}/locations/us-central1/glossaries/{this._glossaryName}";
        translateTextRequest.GlossaryConfig = new TranslateTextGlossaryConfig()
        {
          Glossary = str2,
          IgnoreCase = true
        };
      }
      TranslateTextResponse translateTextResponse = await this._googleV3Client.TranslateTextAsync(translateTextRequest, (CallSettings) null);
      string str3 = Enumerable.FirstOrDefault<Google.Cloud.Translate.V3.Translation>((IEnumerable<Google.Cloud.Translate.V3.Translation>) translateTextResponse.Translations)?.TranslatedText ?? text;
      if (this.ShouldUseGlossary(sourceLang, targetLang) && translateTextResponse.GlossaryTranslations != null && translateTextResponse.GlossaryTranslations.Count > 0)
      {
        string translatedText = Enumerable.FirstOrDefault<Google.Cloud.Translate.V3.Translation>((IEnumerable<Google.Cloud.Translate.V3.Translation>) translateTextResponse.GlossaryTranslations)?.TranslatedText;
        if (!string.IsNullOrEmpty(translatedText))
          str3 = translatedText;
      }
      return str3;
    }
    catch (Exception ex)
    {
      if (this.ShouldUseGlossary(sourceLang, targetLang) && ex.Message.Contains("glossary"))
      {
        try
        {
          TranslateTextRequest translateTextRequest = new TranslateTextRequest()
          {
            Parent = ((object) new LocationName(this._projectId, "global")).ToString(),
            TargetLanguageCode = targetLang.ToLower(),
            Contents = {
              text
            }
          };
          if ((sourceLang != "auto") && !string.IsNullOrEmpty(sourceLang))
            translateTextRequest.SourceLanguageCode = sourceLang.ToLower();
          return Enumerable.FirstOrDefault<Google.Cloud.Translate.V3.Translation>((IEnumerable<Google.Cloud.Translate.V3.Translation>) (await this._googleV3Client.TranslateTextAsync(translateTextRequest, (CallSettings) null)).Translations)?.TranslatedText ?? text;
        }
        catch (Exception ex1)
        {
        }
      }
      throw new Exception("Google Cloud Translation V3 failed: " + ex.Message, ex);
    }
  }

  public async Task<List<string>> TranslateBatchV3Async(
    List<string> texts,
    string sourceLang = "auto",
    string targetLang = "en")
  {
    if (this._googleV3Client == null || string.IsNullOrEmpty(this._projectId))
      throw new InvalidOperationException("Google V3 client not initialized");
    if (texts == null || texts.Count == 0)
      return new List<string>();
    try
    {
      string parent = ((object) new LocationName(this._projectId, "global")).ToString();
      List<string> allTranslations = new List<string>();
      for (int startIndex = 0; startIndex < texts.Count; startIndex += 100)
      {
        List<string> chunk = Enumerable.ToList<string>(Enumerable.Take<string>(Enumerable.Skip<string>((IEnumerable<string>) texts, startIndex), 100));
        if (Enumerable.Sum<string>((IEnumerable<string>) chunk, (Func<string, int>) (t => Encoding.UTF8.GetByteCount(t ?? ""))) > 25000)
          chunk = Enumerable.ToList<string>(Enumerable.Take<string>((IEnumerable<string>) chunk, 50));
        TranslateTextRequest request = new TranslateTextRequest()
        {
          Parent = parent,
          TargetLanguageCode = targetLang.ToLower()
        };
        request.Contents.AddRange((IEnumerable<string>) chunk);
        if ((sourceLang != "auto") && !string.IsNullOrEmpty(sourceLang))
          request.SourceLanguageCode = sourceLang.ToLower();
        bool usingGlossary = false;
        if (this.ShouldUseGlossary(sourceLang, targetLang))
        {
          string str = $"projects/{this._projectId}/locations/us-central1/glossaries/{this._glossaryName}";
          request.GlossaryConfig = new TranslateTextGlossaryConfig()
          {
            Glossary = str,
            IgnoreCase = true
          };
          usingGlossary = true;
        }
        TranslateTextResponse response = (TranslateTextResponse) null;
        try
        {
          response = await this._googleV3Client.TranslateTextAsync(request, (CallSettings) null);
        }
        catch (Exception ex) when (usingGlossary && ex.Message.Contains("glossary"))
        {
          request.GlossaryConfig = (TranslateTextGlossaryConfig) null;
          response = await this._googleV3Client.TranslateTextAsync(request, (CallSettings) null);
        }
        if (response.Translations.Count != chunk.Count)
        {
          List<string> stringList = new List<string>();
          for (int index = 0; index < chunk.Count; ++index)
          {
            if (index < response.Translations.Count)
              stringList.Add(response.Translations[index].TranslatedText ?? chunk[index]);
            else
              stringList.Add(chunk[index]);
          }
          allTranslations.AddRange((IEnumerable<string>) stringList);
        }
        else
          allTranslations.AddRange((IEnumerable<string>) Enumerable.ToList<string>(Enumerable.Select<Google.Cloud.Translate.V3.Translation, string>((IEnumerable<Google.Cloud.Translate.V3.Translation>) response.Translations, (Func<Google.Cloud.Translate.V3.Translation, string>) (t => t.TranslatedText ?? ""))));
        if (startIndex + 100 < texts.Count)
          await Task.Delay(100);
        chunk = (List<string>) null;
        request = (TranslateTextRequest) null;
        response = (TranslateTextResponse) null;
      }
      if (allTranslations.Count != texts.Count)
        throw new InvalidOperationException($"Batch translation result count mismatch: expected {texts.Count}, got {allTranslations.Count}");
      return allTranslations;
    }
    catch (Exception ex)
    {
      throw new Exception("Google Cloud Translation V3 batch failed: " + ex.Message, ex);
    }
  }

  private bool ShouldUseGlossary(string sourceLang, string targetLang)
  {
    string lower1 = sourceLang?.ToLower();
    string lower2 = targetLang?.ToLower();
    if ((lower1 == "en") && (lower2 == "ru"))
      return true;
    return (lower1 == "ru") && (lower2 == "en");
  }

  private async Task EnsureGlossaryExistsAsync()
  {
    if (this._googleV3Client == null)
      return;
    if (string.IsNullOrEmpty(this._projectId))
      return;
    try
    {
      string glossaryPath = $"projects/{this._projectId}/locations/us-central1/glossaries/{this._glossaryName}";
      try
      {
        Glossary glossaryAsync = await this._googleV3Client.GetGlossaryAsync(new GetGlossaryRequest()
        {
          Name = glossaryPath
        }, (CallSettings) null);
        return;
      }
      catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
      {
      }
      string str = ((object) new LocationName(this._projectId, "us-central1")).ToString();
      Operation<Glossary, CreateGlossaryMetadata> operation = await (await this._googleV3Client.CreateGlossaryAsync(new CreateGlossaryRequest()
      {
        Parent = str,
        Glossary = new Glossary()
        {
          Name = glossaryPath,
          LanguagePair = new Glossary.Types.LanguageCodePair()
          {
            SourceLanguageCode = "en",
            TargetLanguageCode = "ru"
          },
          InputConfig = new GlossaryInputConfig()
          {
            GcsSource = new GcsSource()
            {
              InputUri = this._bucketUri
            }
          }
        }
      }, (CallSettings) null)).PollUntilCompletedAsync((PollSettings) null, (CallSettings) null, (Action<CreateGlossaryMetadata>) null);
      glossaryPath = (string) null;
    }
    catch (Exception ex)
    {
    }
  }

  public async Task<bool> ValidateGoogleCloudGlossaryAsync()
  {
    if (this._apiType != ApiType.GoogleCloudV3 || this._googleV3Client == null || string.IsNullOrEmpty(this._projectId))
      return false;
    try
    {
      string str = $"projects/{this._projectId}/locations/us-central1/glossaries/{this._glossaryName}";
      return (await this._googleV3Client.GetGlossaryAsync(new GetGlossaryRequest()
      {
        Name = str
      }, (CallSettings) null)).EntryCount > 0;
    }
    catch
    {
      return false;
    }
  }

  public async Task<string> CreateGlossaryAsync()
  {
    if (this._apiType != ApiType.GoogleCloudV3 || this._googleV3Client == null || string.IsNullOrEmpty(this._projectId))
      return "Error: Google Cloud V3 not properly initialized";
    try
    {
      await this.EnsureGlossaryExistsAsync();
      return "Glossary creation initiated successfully from Cloud Storage bucket";
    }
    catch (Exception ex)
    {
      return "Error creating glossary from bucket: " + ex.Message;
    }
  }

  private string GetProjectIdFromJson(string jsonPath)
  {
    try
    {
      JsonElement rootElement = JsonDocument.Parse(File.ReadAllText(jsonPath), new JsonDocumentOptions()).RootElement;
      JsonElement jsonElement;
      if (!rootElement.TryGetProperty("project_id", out jsonElement))
        throw new InvalidOperationException("project_id not found in JSON file");
      return jsonElement.GetString() ?? throw new InvalidOperationException("project_id is null");
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException("Failed to extract project_id from JSON: " + ex.Message, ex);
    }
  }

  private async Task<string> TranslateWithGenericApi(
    string text,
    string sourceLang,
    string targetLang)
  {
    HttpResponseMessage httpResponseMessage = await this._httpClient.PostAsync(this._apiKey, (HttpContent) new StringContent(JsonSerializer.Serialize(new
    {
      text = text,
      source_lang = sourceLang,
      target_lang = targetLang
    }, (JsonSerializerOptions) null), Encoding.UTF8, "application/json"));
    httpResponseMessage.EnsureSuccessStatusCode();
    return JsonSerializer.Deserialize<CustomApiTranslator.CustomApiResponse>(await httpResponseMessage.Content.ReadAsStringAsync(), (JsonSerializerOptions) null)?.translated_text ?? text;
  }

  public async Task<Dictionary<string, string>> GetSupportedLanguagesAsync()
  {
    Dictionary<string, string> supportedLanguagesAsync;
    switch (this._apiType)
    {
      case ApiType.DeepL:
        Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
        dictionary1.Add("BG", "Bulgarian");
        dictionary1.Add("CS", "Czech");
        dictionary1.Add("DA", "Danish");
        dictionary1.Add("DE", "German");
        dictionary1.Add("EL", "Greek");
        dictionary1.Add("EN", "English");
        dictionary1.Add("ES", "Spanish");
        dictionary1.Add("ET", "Estonian");
        dictionary1.Add("FI", "Finnish");
        dictionary1.Add("FR", "French");
        dictionary1.Add("HU", "Hungarian");
        dictionary1.Add("ID", "Indonesian");
        dictionary1.Add("IT", "Italian");
        dictionary1.Add("JA", "Japanese");
        dictionary1.Add("KO", "Korean");
        dictionary1.Add("LT", "Lithuanian");
        dictionary1.Add("LV", "Latvian");
        dictionary1.Add("NB", "Norwegian");
        dictionary1.Add("NL", "Dutch");
        dictionary1.Add("PL", "Polish");
        dictionary1.Add("PT", "Portuguese");
        dictionary1.Add("RO", "Romanian");
        dictionary1.Add("RU", "Russian");
        dictionary1.Add("SK", "Slovak");
        dictionary1.Add("SL", "Slovenian");
        dictionary1.Add("SV", "Swedish");
        dictionary1.Add("TR", "Turkish");
        dictionary1.Add("UK", "Ukrainian");
        dictionary1.Add("ZH", "Chinese");
        supportedLanguagesAsync = await Task.FromResult<Dictionary<string, string>>(dictionary1);
        break;
      case ApiType.GoogleCloud:
        Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
        dictionary2.Add("auto", "Auto-detect");
        dictionary2.Add("en", "English");
        dictionary2.Add("ru", "Russian");
        dictionary2.Add("de", "German");
        dictionary2.Add("fr", "French");
        dictionary2.Add("es", "Spanish");
        dictionary2.Add("it", "Italian");
        dictionary2.Add("ja", "Japanese");
        dictionary2.Add("ko", "Korean");
        dictionary2.Add("zh", "Chinese");
        dictionary2.Add("ar", "Arabic");
        dictionary2.Add("hi", "Hindi");
        dictionary2.Add("pt", "Portuguese");
        dictionary2.Add("pl", "Polish");
        dictionary2.Add("nl", "Dutch");
        dictionary2.Add("sv", "Swedish");
        dictionary2.Add("da", "Danish");
        dictionary2.Add("no", "Norwegian");
        dictionary2.Add("fi", "Finnish");
        supportedLanguagesAsync = await Task.FromResult<Dictionary<string, string>>(dictionary2);
        break;
      case ApiType.GoogleCloudV3:
        Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
        dictionary3.Add("auto", "Auto-detect");
        dictionary3.Add("en", "English");
        dictionary3.Add("ru", "Russian");
        dictionary3.Add("de", "German");
        dictionary3.Add("fr", "French");
        dictionary3.Add("es", "Spanish");
        dictionary3.Add("it", "Italian");
        dictionary3.Add("ja", "Japanese");
        dictionary3.Add("ko", "Korean");
        dictionary3.Add("zh", "Chinese");
        dictionary3.Add("ar", "Arabic");
        dictionary3.Add("hi", "Hindi");
        dictionary3.Add("pt", "Portuguese");
        dictionary3.Add("pl", "Polish");
        dictionary3.Add("nl", "Dutch");
        dictionary3.Add("sv", "Swedish");
        dictionary3.Add("da", "Danish");
        dictionary3.Add("no", "Norwegian");
        dictionary3.Add("fi", "Finnish");
        supportedLanguagesAsync = await Task.FromResult<Dictionary<string, string>>(dictionary3);
        break;
      default:
        Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
        dictionary4.Add("auto", "Auto-detect");
        dictionary4.Add("en", "English");
        dictionary4.Add("ru", "Russian");
        dictionary4.Add("de", "German");
        dictionary4.Add("fr", "French");
        dictionary4.Add("es", "Spanish");
        dictionary4.Add("it", "Italian");
        dictionary4.Add("ja", "Japanese");
        dictionary4.Add("ko", "Korean");
        dictionary4.Add("zh", "Chinese");
        supportedLanguagesAsync = await Task.FromResult<Dictionary<string, string>>(dictionary4);
        break;
    }
    return supportedLanguagesAsync;
  }

  public void Dispose()
  {
    this._httpClient?.Dispose();
    if (!(this._googleV3Client is IDisposable googleV3Client))
      return;
    googleV3Client.Dispose();
  }

  private class CustomApiResponse
  {
    public string? translated_text { get; set; }
  }

  private class DeepLResponse
  {
    public CustomApiTranslator.DeepLTranslation[]? translations { get; set; }
  }

  private class DeepLTranslation
  {
    public string? text { get; set; }

    public string? detected_source_language { get; set; }
  }

  private class GoogleResponse
  {
    public CustomApiTranslator.GoogleData? data { get; set; }
  }

  private class GoogleData
  {
    public CustomApiTranslator.GoogleTranslation[]? translations { get; set; }
  }

  private class GoogleTranslation
  {
    public string? translatedText { get; set; }

    public string? detectedSourceLanguage { get; set; }
  }
}
