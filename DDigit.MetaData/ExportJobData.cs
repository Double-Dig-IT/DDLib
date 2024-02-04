namespace DDigit.MetaData;

public class ExportJobData(ObjectTypeEnum objectType, Stream stream, Encoding encoding, string? fileName, bool trace) : 
  JobData(objectType, stream, encoding, fileName, trace)
{
}