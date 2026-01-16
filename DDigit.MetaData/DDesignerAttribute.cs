namespace DDigit.MetaData;

/// <summary>
/// Annotation for Double Digit Designer data elements
/// </summary>
/// <param name="propertyType"></param>
/// <param name="label"></param>
/// <param name="level"></param>
[AttributeUsage(AttributeTargets.Property)] // restrict to properties
public class DDesigner(DDesignerPropertyTypeEnum propertyType, string? label, int level = 0) : Attribute
{
  /// <summary>
  /// What kind of property is this?
  /// </summary>
  public DDesignerPropertyTypeEnum PropertyType { get; } = propertyType;

  /// <summary>
  /// The label for the property
  /// </summary>
  public string? Label { get; } = label;

  /// <summary>
  /// Level for the property 
  /// TODO: Ask Daan for explanation
  /// </summary>
  public int Level { get; } = level;
}
