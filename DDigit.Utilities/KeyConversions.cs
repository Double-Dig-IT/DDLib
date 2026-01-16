namespace DDigit.Utilities;

public static class KeyConversions
{
  private static readonly DateTime beginDate = new(1900, 1, 1);

  public static int DateTimeStringToInt(string? value)
  {
    // return -1 if we do not have a date
    if (value is null)
    {
      return -1;
    }

    // return number of days since the first of january 1900
    var date = int.TryParse(value, out var year) ? new DateTime(year, 1, 1) : DateTime.Parse(value);
    return (date - beginDate).Days + 1;
  }

  public static string DisplayTermValue(object? value, int length)
  {
    var text = ObjectToString(value);
    return text.Length > length ? text[..length] : text;
  }

  public static string StrippedTermValue(object? value, int length)
  {
    var result = new StringBuilder();
    foreach (var ch in ObjectToString(value))
    {
      if (char.IsLetter(ch) || char.IsDigit(ch))
      {
        result.Append(ch);
        if (result.Length == length)
        {
          break;
        }
      }
    }
    return result.ToString();
  }


  public static List<string> AlphaSplit(string text)
  {
    var result = new List<string>();
    var numeric = false;

    var part = new StringBuilder();
    foreach (var ch in text)
    {
      if (ch >= '0' && ch <= '9')
      {
        if (!numeric)
        {
          if (part.Length > 0)
          {
            result.Add(part.ToString());
            part.Clear();
          }
        }
        numeric = true;
      }
      else
      {
        if (numeric)
        {
          if (part.Length > 0)
          {
            result.Add(part.ToString());
            part.Clear();
          }
        }
        numeric = false;
      }
      part.Append(ch);
    }
    result.Add(part.ToString());
    return result;
  }

  public static string Pad(string text, int length)
    => (text.Length > 0 && (text[0] >= '0' && text[0] <= '9') &&
        text.Length < length) ?
      new string('0', length - text.Length) + text : text;

  public static string AlphaKeyValue(string value, int length)
  {
    var key = new StringBuilder();
    AlphaSplit(value).ForEach(x => key.Append(Pad(x, 10)));
    return key.ToString();
  }

  private static string ObjectToString(object? value) => value != null ? value.ToString()! : string.Empty;

  public static string TermValue(object? value, int length) => DisplayTermValue(value, length).ToLower();

  public static decimal IsoDateToDecimal(object value, DateCompletionEnum dateCompletion) 
    => new IsoDate((string?)value, dateCompletion).ToDecimal();
}
