namespace DDigit.MetaData;

/// <summary>
/// Meta data for a condition row (either current or history)
/// </summary>
public class ConditionFieldData
{
  /// <summary>
  /// Field for the Id of the condition
  /// </summary>
  public FieldData? Id { get; internal set; }

  /// <summary>
  /// Fields for a part of the record
  /// </summary>
  public FieldData? Part { get; internal set; }

  /// <summary>
  /// Field for the condition thesaurus term 
  /// </summary>
  public FieldData? Condition { get; internal set; }

  /// <summary>
  /// Field for the notes 
  /// </summary>
  public FieldData? Notes { get; internal set; }

  /// <summary>
  /// Field for the executor of the condition
  /// </summary>
  public FieldData? CheckName { get; internal set; }

  /// <summary>
  /// Field for the date the condition was registered
  /// </summary>
  public FieldData? Date { get; internal set; }
}
