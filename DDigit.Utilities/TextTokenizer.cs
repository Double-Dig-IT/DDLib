namespace DDigit.Utilities
{
  public class TextTokenizer
  {
    public static List<string> GetWords(string language, object? data)
    {
      var text = data as string;
      var result = new List<string>();

      if (!string.IsNullOrWhiteSpace(text))
      {
        int cp = 0;

        List<StringBuilder> words = [];

        StartNewWord(words);
        while (cp < text.Length)
        {
          var ch = text[cp++];

          if (Ignore(ch))
          {
            continue;
          }

          if (IsSeparator(ch))
          {
            AddWordsToResult(words, result);
            words.Clear();
            StartNewWord(words);
            continue;
          }

          if (IsConcatenator(ch))
          {
            AddWordsToResult(words, result);
            AddCharacterToWords(ch, words);
            StartNewWord(words);
            continue;
          }

          AddCharacterToWords(ch, words);
        }
        AddWordsToResult(words, result);
      }
      return result;
    }

    private static void AddWordsToResult(List<StringBuilder> words, List<string> result)
    {
      words.ForEach(word =>
      {
        if (word.Length > 0)
        {
          result.Add(word.ToString().ToLower());
        }
      });
    }

    private static void StartNewWord(List<StringBuilder> words) => words.Add(new StringBuilder());

    private static void AddCharacterToWords(char ch, List<StringBuilder> words) =>
      words.ForEach(word => word.Append(ch));

    private static bool IsConcatenator(char ch) => Concatenators.Contains(ch);

    private readonly static string Concatenators = "+-'`_/\\";

    private static bool IsSeparator(char ch) => Separators.Contains(ch);

    private readonly static string Separators = " \t\n\r,;<>[]{}()";

    private static bool Ignore(char ch) => IgnoreCharacters.Contains(ch);

    private readonly static string IgnoreCharacters = "?!*&%^#$=";
  }
}