namespace DDigit.MetaData;

public class MetaDataCache
{
  public static ApplicationData? ReadApplication(string folder, bool trace)
    => ReadFromCache<ApplicationData>(ApplicationFileInfo(folder).FullName, trace);

  private static readonly CacheItemPolicy policy = new();

  private static string ApplicationFile(string folder)
    => folder + Path.DirectorySeparatorChar + "adlib.pbk";

  private static FileInfo ApplicationFileInfo(string folder) =>
    new(ApplicationFile(folder));

  public static List<string> GetCacheEntries() => [.. cache.ToFrozenDictionary().Keys];

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

  public static DatabaseData? ReadDatabase(string fileName, bool trace)
  {
    var greaterThan = fileName.IndexOf('>');
    if (greaterThan > 0)
    {
      fileName = fileName[..greaterThan];
    }
    var databaseData = ReadFromCache<DatabaseData>(AddExtension(fileName.Replace('+', Path.DirectorySeparatorChar), DatabaseData.Extension), trace);
    databaseData?.GetRecordMetaDataFields();
    databaseData?.GetLocationFields();
    databaseData?.GetAutoNumberingFields();
    return databaseData;
  }

  static T? ReadFromCache<T>(string fileName, bool trace) where T : FileData, new()
  {
    T? result;
    var fileInfo = new FileInfo(fileName);
    var fullName = fileInfo.FullName;

    var cacheItem = cache.GetCacheItem(fullName);
    if (cacheItem != null)
    {
      result = cacheItem.Value as T;
      if (result != null && fileInfo.LastWriteTime > result.DateTimeWritten)
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
    var result = new T() { FileName = fileName };
    result.Read(trace);
    cache.Add(key, result, policy);
    return result;
  }

  private static string AddExtension(string fileName, string extension) =>
    fileName.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase) ? fileName : fileName + extension;

  private static FormData? ReadForm(string fileName, bool trace) =>
    ReadFromCache<FormData>(AddExtension(fileName, FormData.Extension), trace);

  public static DatabaseData? FirstDatabase(string folder, bool trace)
  {
    var files = new DirectoryInfo(folder).GetFiles($"*{DatabaseData.Extension}");
    return files.Length > 0 ? ReadDatabase(files[0].FullName, trace) : null;
  }

  private static readonly MemoryCache cache = new("metadata");
}
