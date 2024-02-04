namespace DDigit.Utilities;

public static class Languages
{
  private static readonly List<string> languages = ["en", "nl", "fr", "de", "ar", "it", "el", "pt", "ru", "se", "he", "da", "no", "fi", "zh"];

  public static int GetAdlibNo(string language)
  {
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
    if (!languages.Contains(mainLanguage))
    {
      throw new LanguageIsNotSupportedException(language);
    }
    return languages.IndexOf(mainLanguage);
  }
}
