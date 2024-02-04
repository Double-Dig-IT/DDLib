using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task WriteRecordAsync(Record record)
  {
    if (record.Database == null)
    {
      throw new NullReferenceException(nameof(record.Database));
    }
    if (record.Database.Name == null)
    {
      throw new NullReferenceException(nameof(record.Database.Name));
    }
    try
    {
      Repository.StartTransaction(record.Database);

      RunScript(record);

      await record.ResolveLinksAsync();
      if (record.Id == 0)
      {
        record.Id = await Repository.GetNewRecordId(record.Database, record.Dataset);
        await Repository.WriteNewData(record.Database.Name, record.Id, record.Creation, record.Modification, record.Serialize().ToString());
      }
      else
      {
        await Repository.UpdateData(record.Database.Name, record.Id, record.Modification, record.Serialize().ToString());
      }
      await WriteIndexDataAsync(record);
      Repository.Commit();
    }
    catch (Exception)
    {
      Repository.Rollback();
      throw;
    }
  }

  private static void RunScript(Record record)
  {
    var path = record.Database!.BeforeStorageAdapl;
    if (!string.IsNullOrWhiteSpace(path))
    {
      var extension = Path.GetExtension(path);
      if (!string.IsNullOrWhiteSpace(extension))
      {
         path = path[..(path.Length - extension.Length)];
      }
      var scriptPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(record.Database!.PhysicalPath)!, path + ".ps1"));
      if (File.Exists(scriptPath))
      {
        RunPowerShell(scriptPath, record);
      }
    }
  }

  private static void RunPowerShell(string scriptPath, Record record)
  {
    using var runSpace = RunspaceFactory.CreateRunspace();
    runSpace.Open();
    using var pipeLine = runSpace.CreatePipeline();
    var command = new Command(scriptPath);
    command.Parameters.Add("Record", record);
    pipeLine.Commands.Add(command);
    pipeLine.Invoke();
  }

  private async Task WriteIndexDataAsync(Record record)
  {
    var indexChanges = await record.CreateIndexKeysAsync();
    foreach (var change in indexChanges)
    {
      foreach (var index in change.Modifications)
      {
        await WriteChangesAsync(index);
      }
    }
  }

  private async Task WriteChangesAsync(IndexRow index)
  {
    if (index is IntegerIndexRow row )
    {
      if (index.Count > 0)
      {
        await Repository.AddIndexKey(row.Table, row.Key, row.Id);
      }
      else if (index.Count < 0)
      {
        await Repository.RemoveIndexKey(row.Table, row.Key, row.Id);
      }
      return;
    }

    if (index is TermIndexRow termRow)
    {
      if (index.Count > 0)
      {
        await Repository.AddIndexKey(termRow.Table, termRow.Term, termRow.DisplayTerm,
                                     termRow.Language, termRow.Domain, termRow.Id);
      }
      else if (index.Count < 0)
      {
        await Repository.RemoveIndexKey(termRow.Table, termRow.Term, termRow.DisplayTerm,
                                        termRow.Language, termRow.Domain, termRow.Id);
      }
      return;
    }

    throw new NotImplementedException();
  }
}
