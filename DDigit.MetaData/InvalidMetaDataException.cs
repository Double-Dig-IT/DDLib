namespace DDigit.MetaData
{
  internal class InvalidMetaDataException(ObjectTypeEnum objectType, string? fileName, long position, Exception? exception = null)
    : DDException($"Invalid object type '{objectType}' in '{fileName}' location {position:n0} {exception?.Message}")
  {
    public ObjectTypeEnum ObjectType { get; } = objectType;
    public string? FileName { get; } = fileName;
    public long Position { get; } = position;
    public string? InnerMessage { get; } = exception?.Message;
  }
}