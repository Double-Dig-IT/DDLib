namespace DDigit.Utilities;

public static class Extensions
{
  public static bool Match(this string text, string pattern, bool caseSensitive)
  {
    bool match = true;
    int afterLastWild = -1; // Location after last "*", if we've encountered one
    int iText = 0;
    int iPattern = 0;

    while (true) // Walk the text strings one character at a time.
    {
      if (iText == text.Length)
      {
        if (iPattern == pattern.Length)
        {
          break;                 // "x" matches "x"
        }
        else if (pattern[iPattern] == '*')
        {
          iPattern++;
          continue;              // "x*" matches "x" or "xy"
        }
        match = false;
        break;                     // "x" doesn't match "xy"
      }
      else
      {
        char t = text[iText];
        char w = iPattern < pattern.Length ? pattern[iPattern] : '\0';

        if (!caseSensitive)
        {
          // Lowercase the characters to be compared.
          t = char.ToLowerInvariant(t);
          w = char.ToLowerInvariant(w);
        }

        if (t != w)
        {
          if (w == '*')
          {
            afterLastWild = ++iPattern;
            continue;              // "*y" matches "xy"
          }
          else if (afterLastWild >= 0)
          {
            iPattern = afterLastWild;
            w = iPattern < pattern.Length ? pattern[iPattern] : '\0';

            if (w == '\0')
            {
              break;             // "*" matches "x"
            }
            else if (t == w)
            {
              iPattern++;
            }
            iText++;
            continue;     // "*sip*" matches "mississippi"
          }
          else
          {
            match = false;
            break;           // "x" doesn't match "y"
          }
        }
      }
      iText++;
      iPattern++;
    }
    return match;
  }

  public static string StripAccents(this string textWithAccents)
  {
    string normalizedString = textWithAccents.Normalize(NormalizationForm.FormD);
    var stringBuilder = new StringBuilder();

    for (int i = 0; i < normalizedString.Length; i++)
    {
      char c = normalizedString[i];
      if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
      {
        stringBuilder.Append(c);
      }
    }

    return stringBuilder.ToString();
  }

  public static string Repeat(this string text, int count)
  {
    var repeatedText = new StringBuilder();
    for (var i = 0; i < count; i++)
    {
      repeatedText.Append(text);
    }
    return repeatedText.ToString();
  }

  public static DateOnly ToDateOnly(this string isoDate)
  {
    string[] isoDateElements = isoDate.Split(['-']);
    int year = Convert.ToInt32(isoDateElements[0]);
    int month = isoDateElements.Length > 1 ? Convert.ToInt32(isoDateElements[1]) : 1;
    int day = isoDateElements.Length > 2 ? Convert.ToInt32(isoDateElements[2]) : 1;

    return new DateOnly(year, month, day);
  }

  public static void AddUnique(this List<string> list, string value)
  {
    if (!list.Contains(value))
    {
      list.Add(value);
    }
  }

  public static bool IsWhiteSpace(this string text, string? whiteSpaceCharacters = null)
  {
    for (int i = 0; i < text.Length; i++)
    {
      if (whiteSpaceCharacters != null && whiteSpaceCharacters.Length > 0)
      {
        if (!whiteSpaceCharacters.Contains(text[i]))
        {
          return false;
        }
      }
      else if (text[i] != ' ')
      {
        return false;
      }
    }
    return true;
  }

  /// <summary>
  /// Divides a list into separate chunks
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="list"></param>
  /// <param name="amountPerChunk">The amount of items per chunk</param>
  /// <returns></returns>
  public static List<List<T>>? ToChunks<T>(this IList<T> list, int amountPerChunk = 2)
  {
    if (amountPerChunk == 0)
    {
      amountPerChunk = 1;
    }

    if (list.Count == 0) return null;
    var chunksList = new List<List<T>>();
    var currentList = new List<T>();

    for (var i = 0; i < list.Count; i++)
    {
      if (i % amountPerChunk == 0 && i != 0)
      {
        chunksList.Add(currentList);
        currentList = [];
      }
      currentList.Add(list[i]);
    }

    if (currentList.Count != 0)
    {
      chunksList.Add(currentList);
    }
    return chunksList;
  }
}