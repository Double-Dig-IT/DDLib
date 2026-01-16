namespace DDigit.MetaData;

/// <summary>
/// Adlib attribute to flag old Adlib properties
/// </summary>
/// <param name="text"></param>
[AttributeUsage(AttributeTargets.All)] // restrict to properties
public class Adlib(string? text = "") : Attribute
{
  /// <summary>
  /// The text for the property
  /// </summary>
  public string? Text { get; } = text;
}
