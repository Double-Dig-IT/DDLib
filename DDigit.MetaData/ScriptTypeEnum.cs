namespace DDigit.MetaData;

/// <summary>
/// When is the field script triggered?
/// </summary>
[Flags]
public enum ScriptTypeEnum
{
  /// <summary>
  /// Never
  /// </summary>
  None = 0x0,

  /// <summary>
  /// Immediately when the field receives focus.
  /// </summary>
  BeforeEdit = 0x1,

  /// <summary>
  /// Immediately when the field looses focus. 
  /// </summary>
  AfterEdit = 0x2
}
