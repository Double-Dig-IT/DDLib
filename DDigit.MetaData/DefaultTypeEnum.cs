namespace DDigit.MetaData;

/// <summary>
/// Determines how a default value is set:
/// </summary>
public enum DefaultTypeEnum
{
    /// <summary>
    /// Not set
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// No default value
    /// </summary>
    None = 1,

    /// <summary>
    /// Use the current date
    /// </summary>
    CurrentDate = 2,

    /// <summary>
    /// Use the current time.
    /// </summary>
    CurrentTime = 3,

    /// <summary>
    /// Use the current user name
    /// </summary>
    UserName = 4,

    /// <summary>
    /// Use a fixed value
    /// </summary>
    Value = 5
}