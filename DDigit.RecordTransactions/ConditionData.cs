namespace DDigit.RecordTransactions;

public class ConditionData
{
  public int Id { get; private set; }
  public string? Part { get; internal set; }
  public string? Condition { get; internal set; }
  public string? Notes { get; internal set; }
  public string? CheckName { get; internal set; }
  public DateOnly? Date { get; internal set; }

  internal static void SetRow(Record record, ConditionFieldData fieldData, ConditionData row, int occurence = 1)
  {
    record.Insert(fieldData.Part!, occurence, value: row.Part);

    if (fieldData.Condition is { } condition)
    {
      record[condition, occurence] = row.Condition;
    }
    if (fieldData.Notes is { } notes)
    {
      record[notes, occurence] = row.Notes;
    }
    if (fieldData.CheckName is { } checkName)
    {
      record[checkName, occurence] = row.CheckName;
    }
    if (fieldData.Date is { } date)
    {
      record[date, occurence] = row.Date?.ToString("yyyy-MM-dd");
    }
  }

  internal static ConditionData GetRow(Record record, ConditionFieldData fields, int occurence = 1)
  {
    try
    {
      var result = new ConditionData
      {
        Id = fields.Id is not null ? GetInt(record[fields.Id, occurence]) : 0,
        Part = fields.Part is not null ? record[fields.Part, occurence] : null,
        Condition = fields.Condition is not null ? record[fields.Condition, occurence] : null,
        Notes = fields.Notes is not null ? record[fields.Notes, occurence] : null,
        CheckName = fields.CheckName is not null ? record[fields.CheckName, occurence] : null,
        Date = fields.Date is not null ? GetDateOnly(record[fields.Date, occurence]) : null
      };

      return result;
    }
    catch (Exception ex)
    {
      throw new DDException($"Error reading condition data: {ex.Message}", ex);
    }
  }

  private static int GetInt(string? value) => !string.IsNullOrEmpty(value) ? int.Parse(value) : 0;

  private static DateOnly? GetDateOnly(string? value) => !string.IsNullOrEmpty(value) ? DateOnly.Parse(value) : null;
}
