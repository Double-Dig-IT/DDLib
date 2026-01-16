namespace DDigit.MetaData;

/// <summary>
/// Contains metata for a an export job
/// </summary>
/// <param name="objectType"></param>
/// <param name="stream"></param>
/// <param name="encoding"></param>
/// <param name="trace"></param>
public class ExportJobData(ObjectTypeEnum objectType, FileStream stream, Encoding encoding, bool trace) :
  JobData(objectType, stream, encoding, trace)
{
}