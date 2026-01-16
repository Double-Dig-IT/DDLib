namespace DDigit.Results;

/// <summary>
/// The result of a move transaction.
/// </summary>
/// <typeparam name="TValue">The return type of the transaction</typeparam>
public sealed record Result<TValue> : Result
{
  /// <summary>
  /// Any resulting record
  /// </summary>
  public TValue? Data { get; set; }

  /// <summary>
  /// Creates a <see cref="Result{T}"/> from an error
  /// </summary>
  /// <param name="severity">The severity of the error</param>
  /// <param name="errorCode">The type of error</param>
  /// <param name="offendingValue">The value that caused the error</param>
  /// <returns>The <see cref="Result{T}"/> with the error</returns>
  public new static Result<TValue> CreateErrorResult(MessageSeverityEnum severity, ErrorCodeEnum errorCode, object offendingValue)
    => new()
    {
      IsValid = false,
      Issues =
      [
        new Issue
        {
          Severity = severity,
          ErrorCode = errorCode,
          OffendingValue = offendingValue,
        }
      ]
    };

  /// <summary>
  /// Creates a new <see cref="Result{T}"/> from an existing error <see cref="Result{T}"/>
  /// </summary>
  /// <param name="existingResult">The existing result to create the new <see cref="Result{T}"/> from</param>
  /// <returns>The newly created <see cref="Result{TPreviousResult}"/></returns>
  /// <typeparam name="TPreviousValue">The return type of the existing result</typeparam>
  public static Result<TValue> FromErrorResult<TPreviousValue>(Result<TPreviousValue> existingResult)
    => new()
    {
      IsValid = false,
      Issues = existingResult.Issues
    };

  /// <summary>
  /// Creates a success <see cref="Result{T}"/>
  /// </summary>
  /// <param name="data">The data to create the result with</param>
  /// <returns>The success <see cref="Result{T}"/></returns>
  public static Result<TValue> CreateSuccessResult(TValue data)
    => new()
    {
      IsValid = true,
      Data = data
    };
}

/// <summary>
/// The result of a move transaction.
/// </summary>
public record Result
{
  /// <summary>
  /// Is this a valid transaction?
  /// </summary>
  public bool IsValid { get; set; }

  /// <summary>
  /// A list of issues with this data, in case of invalid data.
  /// </summary>
  public List<Issue> Issues { get; set; } = [];

  /// <summary>
  /// Does this result have any issues of this type?
  /// </summary>
  /// <param name="errorCode"></param>
  /// <returns></returns>
  public bool HasIssue(ErrorCodeEnum errorCode) => Issues.Any(i => i.ErrorCode == errorCode);

  /// <summary>
  /// Creates a <see cref="Result"/> from an error
  /// </summary>
  /// <param name="severity">The severity of the error</param>
  /// <param name="errorCode">The type of error</param>
  /// <param name="offendingValue">The value that caused the error</param>
  /// <returns>The <see cref="Result"/> with the error</returns>
  public static Result CreateErrorResult(MessageSeverityEnum severity, ErrorCodeEnum errorCode, object offendingValue)
    => new()
    {
      IsValid = false,
      Issues =
      [
        new Issue
        {
          Severity = severity,
          ErrorCode = errorCode,
          OffendingValue = offendingValue,
        }
      ]
    };

  /// <summary>
  /// Creates a new <see cref="Result"/> from an existing error <see cref="Result"/>
  /// </summary>
  /// <param name="existingResult">The existing result to create the new <see cref="Result"/> from</param>
  /// <returns>The newly created <see cref="Result"/></returns>
  public static Result FromErrorResult(Result existingResult)
    => new()
    {
      IsValid = false,
      Issues = existingResult.Issues
    };

  /// <summary>
  /// Creates a success <see cref="Result"/>
  /// </summary>
  /// <returns>The <see cref="Result"/></returns>
  public static Result CreateSuccessResult()
    => new()
    {
      IsValid = true,
    };
}
