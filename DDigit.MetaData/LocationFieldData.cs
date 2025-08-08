namespace DDigit.MetaData;

public class LocationFieldData
{
  public FieldData? Name { get; internal set; }
  public FieldData? Id { get; internal set; }
  public FieldData? Barcode { get; internal set; }
  public FieldData? Context { get; internal set; }
  public FieldData? StartDate { get; internal set; }
  public FieldData? StartTime { get; internal set; }
  public FieldData? EndDate { get; internal set; }
  public FieldData? EndTime { get; internal set; }
  public FieldData? Authorizer { get; internal set; }
  public FieldData? AuthorizerId { get; internal set; }
  public FieldData? Executor { get; internal set; }
  public FieldData? Notes { get; internal set; }
  public FieldData? Type { get; internal set; }
  public FieldData? Suitability { get; internal set; }
}
