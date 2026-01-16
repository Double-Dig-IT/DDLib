namespace DDigit.Utilities;

public class IsoDate
{
  static int LastDayOfMonth(int year, int month)
  => month switch
  {
    1 => 31,
    2 => (year > 0 && year < 10000) && DateTime.IsLeapYear(year) ? 29 : 28,
    3 => 31,
    4 => 30,
    5 => 31,
    6 => 30,
    7 => 31,
    8 => 31,
    9 => 30,
    10 => 31,
    11 => 30,
    12 => 31,
    _ => throw new ArgumentOutOfRangeException(nameof(month))
  };

  public IsoDate(string? text, DateCompletionEnum dateCompletion = DateCompletionEnum.FirstDay)
  {
    Text = text;
    DateCompletion = dateCompletion;
    int year = 0, month = 0, day = 0, pos = 0;
 
    if (text is not null)
    {
      var result = TryValidateBC(text, ref pos, out var bc) &&
                   TryValidateYear(text, ref pos, out year) &&
                   TryValidateMonth(text, ref pos, out month, dateCompletion) &&
                   TryValidateDay(text, ref pos, year, month, out day, dateCompletion);

      if (!result) throw new InvalidIsoDateException(text);

      BC = bc;
      Year = year;
      Month = month;
      Day = day;
    }
  }

  public static bool Validate(string text, DateCompletionEnum dateCompletion = DateCompletionEnum.FirstDay)
  {
    int pos = 0;
    var result = TryValidateBC(text, ref pos, out var _) &&
                 TryValidateYear(text, ref pos, out var year) &&
                 TryValidateMonth(text, ref pos, out var month, dateCompletion) &&
                 TryValidateDay(text, ref pos, year, month, out var _, dateCompletion);
    return result;
  }

  private static bool TryValidateDay(string text, ref int pos, int year, int month, out int day, DateCompletionEnum dateCompletion)
  {
    day = 0;
    if (text.Length > pos && text[pos] == '-')
    {
      pos++;
      if (text.Length >= pos + 2 && int.TryParse(text[pos..(pos + 2)], out int d))
      {
        if (d > 0 && d <= LastDayOfMonth(year, month))
        {
          day = d;
          return true;
        }
        return false;
      }
    }
    if (text.Length <= pos)
    {
      day = dateCompletion == DateCompletionEnum.FirstDay ? 1 : LastDayOfMonth(year, month);
      return true;
    }
    return false;
  }

  private static bool TryValidateMonth(string text, ref int pos, out int month, DateCompletionEnum dateCompletion)
  {
    if (text.Length > pos && text[pos] == '-')
    {
      pos++;
      if (text.Length >= pos + 2 && int.TryParse(text[pos..(pos + 2)], out int m))
      {
        pos += 2;
        month = m;
        return true;
      }
    }
    if (text.Length <= pos)
    {
      month = dateCompletion == DateCompletionEnum.FirstDay ? 1 : 12;
      return true;
    }
    month = 0;
    return false;
  }

  private static bool TryValidateBC(string text, ref int pos, out bool bc)
  {
    while (pos < text.Length && char.IsWhiteSpace(text[pos]))
    {
      pos++;
    }
    if (pos < text.Length && text[pos] == '-')
    {
      bc = true;
      pos++;
      return true;
    }
    if (pos < text.Length && char.IsDigit(text[pos]))
    {
      bc = false;
      return true;
    }
    bc = false;
    return false;
  }

  private static bool TryValidateYear(string text, ref int pos, out int year)
  {
    int start = pos;
    while (pos < text.Length && char.IsDigit(text[pos]))
    {
      pos++;
    }

    if (int.TryParse(text[start..pos], out int y))
    {
      year = y;
      return true;
    }
    year = 0;
    return false;
  }

  public bool BC { get; private set; }
  public int Year { get; private set; }
  public int Month { get; private set; }
  public string? Text { get; private set; }
  public int Day { get; private set; }
  public DateCompletionEnum DateCompletion { get; private set; }

  public decimal ToDecimal()
  {
    decimal result = Year + (decimal)Month / 100 + (decimal)Day / 10000;
    return BC ? -result : result;
  }

  public override string? ToString() => Text;
}
