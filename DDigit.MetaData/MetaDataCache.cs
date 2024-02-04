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
    var extension = new DatabaseData().Extension;
    var result = new List<DatabaseData>();
    foreach (var fileInfo in new DirectoryInfo(workingDirectory).GetFiles($"{databaseName}{extension}"))
    {
      var metaData = ReadDatabase(fileInfo.FullName, trace);
      if (metaData != null)
      {
        result.Add(metaData);
      }
    }
    return result;
  }

  public static IEnumerable<FormData> FindForms(string folder, string? formName = "*", bool trace = false)
  {
    var extension = new FormData().Extension;
    var result = new List<FormData>();
    foreach (var fileInfo in new DirectoryInfo(folder).GetFiles($"{formName}{extension}"))
    {
      var metaData = ReadForm(fileInfo.FullName, trace);
      if (metaData != null)
      {
        result.Add(metaData);
      }
    }
    return result;
  }

  public static DatabaseData? ReadDatabase(string folder, string database, bool trace) =>
    ReadDatabase($"{folder}{Path.DirectorySeparatorChar}{database}{new DatabaseData().Extension}", trace);

  public static DatabaseData? ReadDatabase(string fileName, bool trace)
  {
    int greaterThan = fileName.IndexOf('>');
    if (greaterThan > 0)
    {
      fileName = fileName[..greaterThan];
    }
    var extension = new DatabaseData().Extension;
    return ReadFromCache<DatabaseData>(AddExtension(fileName.Replace('+', Path.DirectorySeparatorChar), extension), trace);
  }

  static T? ReadFromCache<T>(string fileName, bool trace) where T : FileData, new()
  {
    T? result = default;
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
    ReadFromCache<FormData>(AddExtension(fileName, new FormData().Extension), trace);

  public static DatabaseData? FirstDatabase(string folder, bool trace)
  {
    var files = new DirectoryInfo(folder).GetFiles($"*{new DatabaseData().Extension}");
    return files.Length > 0 ? ReadDatabase(files[0].FullName, trace) : null;
  }

  private static readonly MemoryCache cache = new("metadata");
}
