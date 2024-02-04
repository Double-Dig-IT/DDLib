namespace DDigit.MetaData;

public abstract class FileData : BaseData
{

  protected FileData(ObjectTypeEnum objectType, string? fileName, bool trace) : base(objectType, fileName)
  {
    if (fileName != null)
    {
      Read(fileName, trace);
    }
  }

  public void Read(bool trace) => Read(FileName ?? throw new NullReferenceException(nameof(FileName)), trace);

  protected void Read(string fileName, bool trace)
  {
    var fileInfo = new FileInfo(fileName);
    PhysicalPath = fileInfo.FullName;
    Folder = fileInfo.DirectoryName;

    if (!fileInfo.Exists)
    {
      throw new FileNotFoundException($"File '{fileName}' does not exist");
    }

    DateTimeWritten = fileInfo.LastWriteTime;
    using var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
    byte[] bytes = new byte[fs.Length];
    fs.Read(bytes, 0, bytes.Length);
    using var memoryStream = new MemoryStream(bytes, 0, bytes.Length, false, true);
    Decode(memoryStream, trace);
  }

  public void Write(string fileName)
  {
    using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
    Encode(fs);
  }

  public void Save()
  {
    if (FileName == null)
    {
      throw new NullReferenceException(nameof(FileName));
    }
    Write(FileName!);
  }

  /// <summary>
  /// Magic number was used in the past to determine the character set of the metadata, should all be UTF-8 now
  /// </summary>
  internal int Magic
  {
    get; set;
  }

  protected abstract void Decode(Stream stream, bool trace);

  protected abstract void Encode(Stream stream);

  [JsonIgnore]
  public string? PhysicalPath { get; private set; }

  [JsonIgnore]
  public string? Folder { get; private set; }

  [JsonIgnore]
  public DateTime DateTimeWritten { get; private set; }

  protected Encoding TextEncoding = Encoding.UTF8;

  public abstract string Extension { get; }
}
