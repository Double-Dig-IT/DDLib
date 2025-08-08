namespace DDigit.MetaData;

public class OutputJobData : JobData
{
  public OutputJobData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace)
    : base(objectType, stream, encoding, fileName, trace)
  {
  }

  public OutputJobData() : base(ObjectTypeEnum.OutputJob)
  {
  }
}