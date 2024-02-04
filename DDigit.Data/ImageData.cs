namespace DDigit.Data;

public class ImageData(string? database, FieldData field)
{
  public string? Database
  {
    get; set;
  } = database;

  public string? Tag
  {
    get; set;
  } = field.Tag;

  public string? Field
  {
    get; set;
  } = field.Name;

  public string? RetrievalPath
  {
    get; set;
  } = field.RetrievalPath;

  public string? ThumbnailPath
  {
    get; set;
  } = field.ThumbnailRetrievalPath;

  public string? StoragePath
  {
    get; set;
  } = field.FormatString;
}
