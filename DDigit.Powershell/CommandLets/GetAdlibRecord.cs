namespace DDigit.Scripting.CommandLets;

/// <summary>
/// Get a single record
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.Record)]
[OutputType(typeof(Record))]
public class GetAdlibRecord : DDCmdlet
{
  /// <summary>
  /// The database to retrieve the record from
  /// </summary>
  [Parameter(Mandatory = true)]
  public required string Database
  {
    get; set;
  }

  /// <summary>
  /// The record number to retrieve
  /// </summary>
  [Parameter(Mandatory = true)]
  public int Id
  {
    get; set;
  }

  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    IRecord? record = null;

    async Task Search()
    {
      record = await ReadAsync();
    }
    RunAsyncTask(Search);

    if (SessionState != null)
    {
      WriteObject(record);
    }
  }

  /// <summary>
  /// Read a record
  /// </summary>
  /// <returns>record or null</returns>
  public async Task<Record?> ReadAsync() =>
    await provider.ReadRecordAsync(WorkingDirectory, Database, Id, default);

  public static Record? Read(string folder, string database, int id)
  {
    var provider = new DDataProvider(new MSSqlRepository());
    var databaseData = provider.GetDatabase(folder, database) ??
      throw new DatabaseNotFoundException(folder, database);
    var task = provider.ReadRecordAsync(databaseData, id);
    task.Wait();
    var record = task.Result;
    return record;
  }
}
