namespace DDigit.MetaData;

/// <summary>
/// Metadata Data for a location row (either current or history)
/// </summary>
public class LocationFieldData
{
  /// <summary>
  /// Field for the Name of the location
  /// </summary>
  public FieldData? Name { get; internal set; }

  /// <summary>
  /// Field for the Id of the location
  /// </summary>
  public FieldData? Id { get; internal set; }

  /// <summary>
  /// Field for the barcode
  /// </summary>
  public FieldData? Barcode { get; internal set; }

  /// <summary>
  /// Field for the path of the location (hierarchical)
  /// </summary>
  public FieldData? Context { get; internal set; }

  /// <summary>
  /// Field to store the date when the object was moved
  /// </summary>
  public FieldData? StartDate { get; internal set; }

  /// <summary>
  /// Field for the time when the object was moved
  /// </summary>
  public FieldData? StartTime { get; internal set; }

  /// <summary>
  /// Field for the date when the object was removed
  /// </summary>
  public FieldData? EndDate { get; internal set; }

  /// <summary>
  /// Time when the object was removed
  /// </summary>
  public FieldData? EndTime { get; internal set; }

  /// <summary>
  /// Field for the person who authorized the move
  /// </summary>
  public FieldData? Authorizer { get; internal set; }

  /// <summary>
  ///  Field for the Id of the authorizer
  /// </summary>
  public FieldData? AuthorizerId { get; internal set; }

  /// <summary>
  /// Field for the person who moved the object
  /// </summary>
  public FieldData? Executor { get; internal set; }

  /// <summary>
  /// Field for notes regarding the move
  /// </summary>
  public FieldData? Notes { get; internal set; }

  /// <summary>
  /// Field for the location type (package -or- location)
  /// </summary>
  public FieldData? Type { get; internal set; }

  /// <summary>
  /// Field for the suitability of the location
  /// </summary>
  public FieldData? Suitability { get; internal set; }
}
