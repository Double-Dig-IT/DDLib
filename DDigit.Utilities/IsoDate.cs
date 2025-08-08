
namespace DDigit.Utilities;

public class IsoDate
{
  public IsoDate(string? text, DateCompletionEnum dateCompletion = DateCompletionEnum.FirstDay)
  {
    if (string.IsNullOrWhiteSpace(text))
    {
      throw new InvalidIsoDateException(text);
    }

    Text = text;
    DateCompletion = dateCompletion;
    GetBC();
    GetYear();
    GetMonth();
    GetDay();
  }

  private void GetBC()
  {
    BC = Text[i] == '-';
    if (BC)
    {
      i++;
    }
  }

  private void GetYear()
  {
    var y = new StringBuilder();
    while (i < Text.Length && char.IsDigit(Text[i]))
    {
      y.Append(Text[i++]);
    }

    if (int.TryParse(y.ToString(), out int year))
    {
      Year = year;
      return;
    }
    else
    {
      throw new InvalidIsoDateException(Text);
    }
  }

  private void GetMonth()
  {
    //   int month = dateCompletion == DateCompletionEnum.FirstDay ? 1 : 12;
    if (Text.Length > i && Text[i] == '-')
    {
      i++;
      if (Text.Length >= i + 2 && int.TryParse(Text[i..(i + 2)], out int month))
      {
        i += 2;
        Month = month;
        return;
      }
    }
    if (Text.Length <= i)
    {
      Month = DateCompletion == DateCompletionEnum.FirstDay ? 1 : 12;
      return;
    }
    throw new InvalidIsoDateException(Text);
  }

  private void GetDay()
  {
    // int day = dateCompletion == DateCompletionEnum.FirstDay ? 1 : 31;
    if (Text.Length > i && Text[i] == '-')
    {
      i++;
      if (Text.Length >= i + 2 && int.TryParse(Text[i..(i + 2)], out int day))
      {
        Day = day;
        return;
      }
    }
    if (Text.Length <= i)
    {
      Day = DateCompletion == DateCompletionEnum.FirstDay ? 1 : LastDayOfMonth(Year, Month);
      return;
    }
    throw new InvalidIsoDateException(Text);
  }

  private static int LastDayOfMonth(int year, int month)
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

  private int i;

  public bool BC { get; private set; }
  public int Year { get; private set; }
  public int Month { get; private set; }
  public string Text { get; private set; }
  public int Day { get; private set; }
  public DateCompletionEnum DateCompletion { get; private set; }

  public decimal ToDecimal()
  {
    decimal result = Year + (decimal)Month / 100 + (decimal)Day / 10000;
    return BC ? -result : result;
  }

  public override string ToString() => Text;
}
