namespace DDigit.MetaData;

public enum TemplateTypeEnum
{
  /// <summary>
  /// A normal page of output.
  /// </summary>
  Normal = 0,
  /// <summary>
  /// A page of labels.
  /// </summary>
  Label = 1,
  /// <summary>
  /// Raw (used to be Ipl; template for an Ipl printer).
  /// </summary>
  Raw = 2,
  /// <summary>
  /// Inline.
  /// </summary>
  Inline = 3,
  /// <summary>
  /// Custom.
  /// </summary>
  Custom = 4
}