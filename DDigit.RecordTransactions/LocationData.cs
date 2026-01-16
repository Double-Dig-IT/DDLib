namespace DDigit.RecordTransactions;

public class LocationData
{
  public string? Authorizer { get; private set; }
  public int AuthorizerId { get; private  set; }
  public string? Barcode { get; private set; } 
  public string? Context { get; private set; }
  public DateOnly? EndDate { get; internal set; }
  public TimeOnly? EndTime { get; internal set; } 
  public string? Executor { get; internal set; }
  public int Id { get; set; } 
  public string? Name { get; private set; }
  public string? Notes { get; internal set; }
  public string? Suitability { get; private set; } 
  public string? Type { get; private set; }
  public DateOnly? StartDate { get; internal set; } 
  public TimeOnly? StartTime { get; internal set; }
  public override string ToString() =>
    $"{Context} {Barcode} {StartDate} {EndDate} {Executor} {Notes}";

  static int GetInt(string? value) => !string.IsNullOrEmpty(value) ? int.Parse(value) : 0;
  static DateOnly? GetDateOnly(string? value) => !string.IsNullOrEmpty(value) ? DateOnly.Parse(value) : null;
  static TimeOnly? GetTimeOnly(string? value) => !string.IsNullOrEmpty(value) ? TimeOnly.Parse(value) : null;

  internal static void SetRow(Record record, LocationFieldData fields, LocationData row, int occ = 1)
  {
    // To prevent resolving a linked field, the linked field is set to null.
    // This is to prevent issues with linked fields that are already resolved.
    // record.Set() is used here to set the values in the record because it allows setting of in values.

    if (fields.Authorizer is not null)
    {
      record.Set(fields.Authorizer!, occ, null);
    }
    if (fields.AuthorizerId is not null)
    {
      record.Set(fields.AuthorizerId, occ, row.AuthorizerId);
    }
    if (CanWrite(fields.Barcode))
    {
      record.Set(fields.Barcode!, occ, row.Barcode);
    }
    if (fields.EndDate is not null)
    {
      record.Set(fields.EndDate, occ, row.EndDate?.ToString("yyyy-MM-dd"));
    }
    if (fields.EndTime is not null)
    {
      record.Set(fields.EndTime, occ, row.EndTime?.ToString("HH:mm:ss"));
    }
    if (fields.Executor is not null)
    {
      record.Set(fields.Executor, occ, row.Executor);
    }
    if (fields.Name is not null)
    {
      record.Set(fields.Name, occ, null);
    }
    if (fields.Id is not null)
    {
      record.Set(fields.Id, occ, row.Id);
    }
    if (fields.Notes is not null)
    {
      record.Set(fields.Notes, occ, row.Notes);
    }
    if (fields.Suitability is not null)
    {
      record.Set(fields.Suitability, occ, row.Suitability);
    }
    if (fields.Type is not null)
    {
      record.Set(fields.Type, occ, row.Type);
    }
    if (fields.StartDate is not null)
    {
      record.Set(fields.StartDate, occ, row.StartDate?.ToString("yyyy-MM-dd"));
    }
    if (fields.StartTime is not null)
    {
      record.Set(fields.StartTime, occ, row.StartTime?.ToString("HH:mm:ss"));
    }
  }

  private static bool CanWrite(FieldData? field) => field is not null && (field.IsWriteBackField || !field.IsMergedField);

  internal static LocationData GetRow(Record record, LocationFieldData fields, int occ = 1)
  {
    try
    {
      var result = new LocationData
      {
        Authorizer = fields.Authorizer is not null ? record[fields.Authorizer, occ] : null,
        AuthorizerId = fields.AuthorizerId is not null ? GetInt(record[fields.AuthorizerId, occ]) : 0,
        Barcode = fields.Barcode is not null ? record[fields.Barcode, occ] : null,
        Context = fields.Context is not null ? record[fields.Context, occ] : null,
        EndDate = fields.EndDate is not null ? GetDateOnly(record[fields.EndDate, occ]) : null,
        EndTime = fields.EndTime is not null ? GetTimeOnly(record[fields.EndTime, occ]) : null,
        Executor = fields.Executor is not null ? record[fields.Executor, occ] : null,
        Id = fields.Id is not null ? GetInt(record[fields.Id, occ]) : 0,
        Name = fields.Name is not null ? record[fields.Name, occ] : null,
        Notes = fields.Notes is not null ? record[fields.Notes, occ] : null,
        Suitability = fields.Suitability is not null ? record[fields.Suitability, occ] : null,
        Type = fields.Type is not null ? record[fields.Type, occ] : null,
        StartDate = fields.StartDate is not null ? GetDateOnly(record[fields.StartDate, occ]) : null,
        StartTime = fields.StartTime is not null ? GetTimeOnly(record[fields.StartTime, occ]) : null
      };

      return result;
    }
    catch (Exception ex)
    {
      throw new DDException($"Error reading location data: {ex.Message}", ex);
    } 
  }
  
}
