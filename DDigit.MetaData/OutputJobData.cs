namespace DDigit.MetaData;

public class OutputJobData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) :
  JobData(objectType, stream, encoding, fileName, trace)
{
}