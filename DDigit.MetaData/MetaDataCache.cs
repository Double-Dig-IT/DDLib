namespace DDigit.MetaData;

/// <summary>
/// A cache that keeps all metadata objects.
/// </summary>
public class MetaDataCache
{
  /// <summary>
  /// Read an application object
  /// </summary>
  /// <param name="folder"></param>
  /// <param name="trace"></param>
  /// <returns></returns>
  public static ApplicationData? ReadApplication(string folder, bool trace)
    => ReadFromCache<ApplicationData>(ApplicationFileInfo(folder).FullName, trace);

  private static readonly CacheItemPolicy policy = new();

  private static string ApplicationFile(string folder)
    => folder + Path.DirectorySeparatorChar + "adlib.pbk";

  private static FileInfo ApplicationFileInfo(string folder) =>
    new(ApplicationFile(folder));

  /// <summary>
  /// Retrieve a list of names objects in the cache
  /// </summary>
  /// <returns>A list of names.</returns>
  public static List<string> GetCacheEntries() => [.. cache.ToFrozenDictionary().Keys];

  /// <summary>
  /// Get a list odf database objects.
  /// </summary>
  /// <param name="workingDirectory"></param>
  /// <param name="databaseName"></param>
  /// <param name="trace"></param>
  /// <returns></returns>
  public static IEnumerable<DatabaseData> FindDatabases(string workingDirectory, string? databaseName = "*", bool trace = false)
  {
    var result = new List<DatabaseData>();
    foreach (var fileInfo in new DirectoryInfo(workingDirectory).GetFiles($"{databaseName}{DatabaseData.Extension}"))
    {
      var databaseData = ReadFromCache<DatabaseData>(fileInfo.FullName, trace);
      if (databaseData != null)
      {
        result.Add(databaseData);
      }
    }
    return result;
  }

  /// <summary>
  /// Find forms in the specified folder. The formName can include wildcards (* and ?).
  /// </summary>
  /// <param name="folder">The folder to search in</param>
  /// <param name="formName">Name of the form</param>
  /// <param name="trace">Trace flag</param>
  /// <returns>A list with forms data</returns>
  public static IEnumerable<FormData> FindForms(string folder, string? formName = "*", bool trace = false)
  {
    var result = new List<FormData>();
    foreach (var fileInfo in new DirectoryInfo(folder).GetFiles($"{formName}{FormData.Extension}"))
    {
      var formData = ReadFromCache<FormData>(fileInfo.FullName, trace);
      if (formData != null)
      {
        result.Add(formData);
      }
    }
    return result;
  }

  /// <summary>
  /// Read a database data object from disk.
  /// </summary>
  /// <param name="folder"></param>
  /// <param name="database"></param>
  /// <param name="trace"></param>
  /// <returns></returns>
  public static DatabaseData? ReadDatabase(string folder, string database, bool trace = false)
  {
    var path = Path.Combine(folder, database);
    var fileInfo = new FileInfo(path);
    if (!fileInfo.Extension.Equals(".inf", StringComparison.OrdinalIgnoreCase))
    {
      path += ".inf";
    }
    return ReadDatabase(path, trace);
  }

  /// <summary>
  /// Read a database definition of disk
  /// </summary>
  /// <param name="fileName"></param>
  /// <param name="trace"></param>
  /// <returns>A DatabaseData object</returns>
  public static DatabaseData? ReadDatabase(string fileName, bool trace = false)
  {
    var greaterThan = fileName.IndexOf('>');
    if (greaterThan > 0)
    {
      fileName = fileName[..greaterThan];
    }
    var databaseData = ReadFromCache<DatabaseData>(AddExtension(fileName.Replace('+', Path.DirectorySeparatorChar), DatabaseData.Extension), trace);
    databaseData?.GetFieldCollections();
    return databaseData;
  }

  static T? ReadFromCache<T>(string fileName, bool trace) where T : FileData, new()
  {
    T? result;
    var fileInfo = new FileInfo(fileName);
    var fullName = fileInfo.FullName;

    var cacheItem = cache.GetCacheItem(fullName);
    if (cacheItem is not null)
    {
      result = cacheItem.Value as T;
      if (result is not null && fileInfo.LastWriteTime > result.DateTimeWritten)
      {
        AddToCache<T>(fullName, fileName, trace);
      }
    }
    else
    {
      if (fileInfo.Exists)
      {
        result = AddToCache<T>(fullName, fileName, trace);
      }
      else
      {
        throw new FileNotFoundException(fullName);
      }
    }
    return result;
  }

  static T AddToCache<T>(string key, string fileName, bool trace) where T : FileData, new()
  {
    var result = new T();
    result.Read(fileName, trace);
    cache.Add(key, result, policy);
    return result;
  }

  private static string AddExtension(string fileName, string extension) =>
    fileName.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase) ? fileName : fileName + extension;

  private static FormData? ReadForm(string fileName, bool trace) =>
    ReadFromCache<FormData>(AddExtension(fileName, FormData.Extension), trace);

  /// <summary>
  /// Get the first database in a certain folder.
  /// </summary>
  /// <param name="folder"></param>
  /// <param name="trace"></param>
  /// <returns></returns>
  public static DatabaseData? FirstDatabase(string folder, bool trace)
  {
    var files = new DirectoryInfo(folder).GetFiles($"*{DatabaseData.Extension}");
    return files.Length > 0 ? ReadDatabase(files[0].FullName, trace) : null;
  }

  private static readonly MemoryCache cache = new("metadata");
}
