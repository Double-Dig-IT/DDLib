namespace DDigit.MetaData;

public class FieldConditionData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  BaseData(objectType, stream, encoding, fileName, Properties, trace)
{
  public ConditionEnum Condition { get; private set; }

  public string? Field { get; private set; }

  public string? Value { get; private set; }

  public ConditionBooleanEnum Boolean { get; private set; }

  public static readonly PropertyList Properties =
  [
     new PropertyMap (0, DataTypesEnum.Int16,  "ElementCount"),
     new PropertyMap (1, DataTypesEnum.Int16,  "Condition", typeof(ConditionEnum)),
     new PropertyMap (2, DataTypesEnum.String, "Field"),
     new PropertyMap (3, DataTypesEnum.String, "Value"),
     new PropertyMap (4, DataTypesEnum.Int16,  "Boolean", typeof(ConditionBooleanEnum))
  ];
}
