namespace DDigit.MetaData;

/// <summary>
/// The parameters for am output job.
/// </summary>
public class OutputJobData : JobData
{
    /// <summary>
    /// Read an output job from a file.
    /// </summary>
    /// <param name="objectType"></param>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <param name="trace"></param>
    public OutputJobData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace)
    : base(objectType, stream, encoding, trace)
  {
  }

  /// <summary>
  /// Create an empty output job.
  /// </summary>
  public OutputJobData() : base(ObjectTypeEnum.OutputJob)
  {
  }
}