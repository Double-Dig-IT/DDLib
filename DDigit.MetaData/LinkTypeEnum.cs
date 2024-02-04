namespace DDigit.MetaData;

public enum LinkTypeEnum
{
  NotLinked = 0,
  Linked = 1,
  [Obsolete("Do not use!For backward compatibility with Adlib for Windows")]
  LocalLink = 2,
  MetaDataLink = 3
}