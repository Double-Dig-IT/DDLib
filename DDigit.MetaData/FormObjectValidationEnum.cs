namespace DDigit.MetaData;

/// <summary>
/// Validation on the screen
/// </summary>
public enum FormObjectValidationEnum : short
{
  /// <summary>
  /// Not defined
  /// </summary>
  Undefined = 0,

  /// <summary>
  /// No validation is performed.
  /// </summary>
  None = 1,

  /// <summary>
  /// The field must contain some data (can not be left blank)
  /// </summary>
  NotEmpty = 2,

  /// <summary>
  /// The field must be complete, i.e. 10 characters if the field is 10 long.
  /// </summary>
  Complete = 3,

  /// <summary>
  /// If a field group member contains any data, then this field must also contain data.
  /// </summary>
  MandatoryGroup = 4,

  /// <summary>
  /// The name an adapl script before this screen is displayed.
  /// </summary>
  BeforeEditAdapl = 5,

  /// <summary>
  /// The name an adapl script after this screen is completed. 
  /// </summary>
  AfterEditAdapl = 6
}
