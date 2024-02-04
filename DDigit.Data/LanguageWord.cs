namespace DDigit.Data;

internal struct LanguageWord
{
  internal LanguageWord(string language, string text)
  {
    Language = language;
    Text = text;
  }

  public override readonly string ToString() => Language + '|' + Text;

  internal string Language
  {
    get; private set;
  }

  internal string Text
  {
    get; private set;
  }
}
