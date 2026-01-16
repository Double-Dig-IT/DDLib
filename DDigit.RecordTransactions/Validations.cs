using DDigit.Utilities;

namespace DDigit.RecordTransactions;

public class Validations
{
  
  public sealed record ValidationResult
  {
    /// <summary>
    /// Is this valid data?
    /// </summary>
    public bool IsValid { get; set; }
    /// <summary>
    /// The field that was validated.
    /// </summary>
    public required FieldData Field { get; set; }
    /// <summary>
    /// A list of issues with this data, in case of invalid data.
    /// </summary>
    public List<Issue> Issues { get; set; } = [];
  }

  /// <summary>
  /// Validate the content of a field, with a default occurrence value of 1.
  /// </summary>
  /// <param name="fieldData"></param>
  /// <param name="value"></param>
  /// <returns></returns>
  public static async Task<ValidationResult> Validate(FieldData fieldData, string value) => await ValidateAsync(fieldData, 1, value);

  /// <summary>
  /// Validate the content of a field.
  /// </summary>
  /// <param name="fieldData">The field definition</param>
  /// <param name="occ">the occurrence number to validate</param>
  /// <param name="value">The value to validate.</param>
  /// <returns>A ValidationResult object</returns>
  public static async Task<ValidationResult> ValidateAsync(FieldData fieldData, int occ, string value, CancellationToken cancellationToken = default)
  {
    var result = new ValidationResult { IsValid = true, Field = fieldData };

    // check if we are trying to resolve a file upload.
    // if so, value might start with a reference to the Temp folder, which will be resolved on record write.
    if (fieldData.Type == FieldTypeEnum.Image || fieldData.Type == FieldTypeEnum.Application)
    {
      value = value.Replace(Path.GetTempPath(), null);
    }

    // check if we are trying to push repeated data in a non-repeated field.
    if (!fieldData.IsRepeated && occ > 1)
    {
      result.IsValid = false;
      result.Issues.Add(new Issue { OffendingValue = value, Severity = MessageSeverityEnum.Error, ErrorCode = ErrorCodeEnum.FieldIsNotRepeated});
    }

    // check the length of the field.
    if (fieldData.Length > 0 && value.Length > fieldData.Length)
    {
      result.IsValid = false;
      result.Issues.Add(new Issue { OffendingValue = value, Severity = MessageSeverityEnum.Error, ErrorCode = ErrorCodeEnum.FieldValueIsTooLong });
    }

    if (fieldData.IsLinked && !fieldData.ForcingAllowed)
    {
      var provider = new DDataProvider(new MSSqlRepository());
      var statement = $"{fieldData.LinkedFieldData!.Name} = '{value}'";
      var searchResult = await provider.SearchAsync(fieldData.LinkedDatabase!, null, statement, null, 1000, cancellationToken);
      if (searchResult?.Ids.Count == 0)
      {
        result.IsValid = false;
        result.Issues.Add(new Issue { OffendingValue = value, Severity = MessageSeverityEnum.Error, ErrorCode = ErrorCodeEnum.ForcingIsNotAllowed });
      }
    }
    
    switch (fieldData.Type)
    {
      case FieldTypeEnum.DateIso:
        if (!IsoDate.Validate(value))
        {
          result.IsValid = false;
          result.Issues.Add(new Issue { OffendingValue = value, Severity = MessageSeverityEnum.Error, ErrorCode = ErrorCodeEnum.InvalidIsoDate });
        }
        break;

      case FieldTypeEnum.Integer:
        if (!int.TryParse(value, out var _))
        {
          result.IsValid = false;
          result.Issues.Add(new Issue { OffendingValue = value, Severity = MessageSeverityEnum.Error, ErrorCode = ErrorCodeEnum.InvalidInteger });
        }
        break;

      case FieldTypeEnum.Float:
        if (!double.TryParse(value, out var _))
        {
          result.IsValid = false;
          result.Issues.Add(new Issue { OffendingValue = value, Severity = MessageSeverityEnum.Error, ErrorCode = ErrorCodeEnum.InvalidNumeric });
        }
        break;

      default:
        break;
    }
    return result;
  }
}
