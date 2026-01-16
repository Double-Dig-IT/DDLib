namespace DDigit.MetaData;

/// <summary>
/// General base class for all file objects.
/// </summary>
public abstract class FileData : BaseData
{
  /// <summary>
  /// The full path of the file where this object is stored. 
  /// </summary>
  [JsonIgnore]
  [DDesigner(DDesignerPropertyTypeEnum.FileName, "Path")]
  public string? FileName { get; set; }

  /// <summary>
  /// Get the folder where this database lives
  /// </summary>
  public string? Folder => Path.GetDirectoryName(FileName);

  /// <summary>
  /// Base contructor for all file objects
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="fileName"></param>
  /// <param name="trace"></param>
  protected FileData(ObjectTypeEnum objectType, string? fileName, bool trace) :
    base(objectType)
  {
    if (fileName is not null)
    {
      Read(fileName, trace);
    }
  }

  /// <summary>
  /// Add an extension to a filename when needed.
  /// </summary>
  /// <param name="fileName"></param>
  /// <param name="extension"></param>
  /// <returns></returns>
  /// <exception cref="NullReferenceException"></exception>
  protected static string AddExtension(string? fileName, string extension)
  {
    if (fileName is null)
    {
      throw new NullReferenceException(nameof(fileName));
    }
    return fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? fileName : fileName + extension;
  }

  /// <summary>
  /// Read the object from disk
  /// </summary>
  /// <param name="fileName"></param>
  /// <param name="trace"></param>
  public void Read(string fileName, bool trace = false)
  {
    FileName = fileName;
    Read(trace);
  }

  /// <summary>
  /// Read an object from disk the filename should have been set.
  /// </summary>
  /// <param name="trace"></param>
  /// <exception cref="FileNotFoundException"></exception>
  public void Read(bool trace = false)
  {
    var fileInfo = new FileInfo(FileName!);

    if (!fileInfo.Exists)
    {
      throw new FileNotFoundException($"File '{FileName}' does not exist");
    }

    DateTimeWritten = fileInfo.LastWriteTime;
    using var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
    Decode(stream, trace);
  }

  /// <summary>
  /// Write an object to disk.
  /// </summary>
  /// <param name="fileName"></param>
  public void Write(string fileName)
  {
    FileName = fileName;
    using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
    Encode(stream);
  }

  /// <summary>
  /// Save an object, filename should be set in FileName
  /// </summary>
  /// <exception cref="NullReferenceException"></exception>
  public void Save()
  {
    if (FileName is null)
    {
      throw new NullReferenceException(nameof(FileName));
    }
    Write(FileName!);
  }

  /// <summary>
  /// Magic number was used in the past to determine the character set of the metadata, should all be UTF-8 now
  /// </summary>
  [Adlib("Magic number was used in the past to determine the character set of the metadata, should all be UTF-8 now")]
  internal int Magic
  {
    get; set;
  } = 32_744;

  /// <summary>
  /// Decode an object.
  /// </summary>
  /// <param name="stream"></param>
  /// <param name="trace"></param>
  protected abstract void Decode(FileStream stream, bool trace);

  /// <summary>
  /// Encode an object
  /// </summary>
  /// <param name="stream"></param>
  protected abstract void Encode(FileStream stream);

  /// <summary>
  /// The date and time that this objct has been written.
  /// </summary>
  [JsonIgnore]
  public DateTime DateTimeWritten { get; private set; }

  /// <summary>
  /// Current Text Encoding of the object.
  /// </summary>
  protected Encoding TextEncoding = Encoding.UTF8;

}
