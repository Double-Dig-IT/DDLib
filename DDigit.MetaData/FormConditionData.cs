namespace DDigit.MetaData;

/// <summary>
/// Form conditions
/// </summary>
/// <param name="objectType"></param>
/// <param name="stream"></param>
/// <param name="encoding"></param>
/// <param name="trace"></param>
public class FormConditionData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
  BaseData(objectType, stream, encoding, Properties, trace)
{
  /// <summary>
  /// The condition to check, equal, unequal, regular expression
  /// </summary>
  public ConditionEnum Condition { get; private set; }

  /// <summary>
  /// Name of the form
  /// </summary>
  public string? FormName { get; private set; }

  /// <summary>
  /// The value to match
  /// </summary>
  public string? Value { get; private set; }

  /// <summary>
  /// boolean condition for
  /// </summary>
  public ConditionBooleanEnum Boolean { get; private set; }

  internal static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,  nameof(ElementCount)),
     new PropertyMap (1, DataTypesEnum.Int16,  nameof(Condition), typeof(ConditionEnum)),
     new PropertyMap (2, DataTypesEnum.String, nameof(Value)),
     new PropertyMap (3, DataTypesEnum.Int16,  nameof(Boolean), typeof(ConditionBooleanEnum)),
     new PropertyMap (4, DataTypesEnum.String, nameof(FormName)),
  ];
}
