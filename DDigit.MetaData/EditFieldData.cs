namespace DDigit.MetaData;

public struct EditFieldData
{
  public FieldData? Name { get; internal set; }
  public FieldData? Date { get; internal set; }
  public FieldData? Time { get; internal set; }
  public FieldData? Source { get; internal set; }
  public FieldData? Notes { get; internal set; }
}
