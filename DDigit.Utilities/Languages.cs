namespace DDigit.Utilities;

public static class Languages
{
  private static readonly Dictionary<string, string> DataLanguages = new()
  {
    { "en", "en-US"},
    { "nl", "nl-NL"}, 
    { "fr", "fr-FR"},
    { "de", "de-DE"},
    { "ar", "ar-SA"},
    { "it", "it-IT"},
    { "el", "el-GR"},
    { "pt", "pt-PT"},
    { "ru", "ru-RU"},
    { "se", "se-SE"},
    { "he", "he-IL"},
    { "da", "da-DK"},
    { "no", "nb-NO"},
    { "fi", "fi-FI"},
    { "zh", "zh-CN"},
    { "es", "es-ES"},
    { "hu", "hu-HU"},
    { "ca", "ca-ES"}
  };

  public static string GetAdlibLang(int occ)
    => DataLanguages.Values.ElementAtOrDefault(occ) ?? throw new LanguageIsNotSupportedException(occ.ToString());

  public static int GetAdlibNo(string language)
  {
    var mainLanguage = GetMainLanguage(language);
    return DataLanguages.Keys.ToList().IndexOf(mainLanguage);
  }

  public static string GetDataLanguage(string language) => language.Length == 2 ? DataLanguages[language] : language;

  public static string GetMainLanguage(string language)
  {
    language = language.ToLower();
    if (language.Length != 2)
    {
      if (language.Length != 5)
      {
        if (language[2] != '-')
        {
          throw new LanguageIsNotSupportedException(language);
        }
      }
    }
    var mainLanguage = language[..2];
    if (!DataLanguages.ContainsKey(mainLanguage))
    {
      throw new LanguageIsNotSupportedException(language);
    }
    return mainLanguage;
  }
}
