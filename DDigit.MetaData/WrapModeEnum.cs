namespace DDigit.MetaData;
/// <summary>
/// Wrap mode
/// </summary>
public enum WrapModeEnum
{
  /// <summary>
  /// No wrapping
  /// </summary>
  Default = 0,
  
  /// <summary>
  /// Show the data on a single line and scroll it horizontally.
  /// </summary>
  SingleLine = 1,

  /// <summary>
  /// Wrap so all lines are shown
  /// </summary>
  MultiLine = 2,
}
