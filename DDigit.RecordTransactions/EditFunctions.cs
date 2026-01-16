namespace DDigit.RecordTransactions;

public class EditFunctions
{
  public async static Task GlobalEditAsync(DatabaseData databaseData, IEnumerable<string> fields,
                              string search, string replacement, IEnumerable<int> ids,
                              bool confirm = false,
                              bool caseSensitive = true,
                              StringReplace.MatchScope scope = StringReplace.MatchScope.WholeString,
                              IGlobalEditCallBacks? callbacks = null,
                              CancellationToken cancellationToken = default)
  {
    bool cancelled = false;
    var provider = new DDataProvider(new MSSqlRepository());
    int total = ids.Count();
    int mileStone = Math.Max(total / (total > 1000 ? 100 : 10), 1);
    int item = 0;

    List<RecordChange> EditRecord(Record record)
    {
      var result = new List<RecordChange>();
      foreach (var field in fields)
      {
        int repCount = record.RepCount(field);
        for (int occ = 1; occ <= repCount; occ++)
        {
          var data = record[field, occ];
          if (data is not null && replacement is not null)
          {
            var replaced = StringReplace.Replace(data, search, replacement, new StringReplace.ReplaceOptions
            {
              UseWildcards = true,
              CaseSensitive = caseSensitive,
              Scope = scope
            }, out int count);

            if (count > 0)
            {
              result.Add(new RecordChange
              {
                Field = field,
                Occ = occ,
                Change = data,
                To = replaced
              });
            }
          }
        }
      }
      return result;
    }

    void UpdateRecord(Record record, List<RecordChange> changes)
    {
      foreach (var change in changes)
      {
        record[change.Field, change.Occ] = change.To;
      }
    }

    if (callbacks is not null)
    {
      await callbacks.ReportAsync(new EditEvent.Started($"Processing", item, total), cancellationToken);
    }

    foreach (var id in ids)
    {
      bool skip = false;
      var record = await provider.ReadRecordAsync(databaseData, id, cancellationToken);
      if (record is not null)
      {
        var changes = EditRecord(record);

        if (confirm && callbacks is not null)
        {
          var request = new ConfirmRequest { RecordId = id, Changes = changes };
          var result = await callbacks.ConfirmAsync(request, cancellationToken);
          switch (result)
          {
            case ConfirmationResult.Accept:
              break;
            case ConfirmationResult.Reject:
              skip = true;
              break;
            case ConfirmationResult.Cancel:
              cancelled = true;
              break;
          }
        }

        if (!skip && changes.Count > 0)
        {
          // update and write the record.
          UpdateRecord(record, changes);
          await record.WriteAsync(cancellationToken);
        }
      }
      if (cancelled)
      {
        break;
      }
      item++;
      if (item % mileStone == 0 && callbacks is not null)
      {
        await callbacks.ReportAsync(new EditEvent.Milestone($"Processing", item, total), cancellationToken);
      }
    }
    if (callbacks is not null)
    {
      await callbacks.ReportAsync(new EditEvent.Completed($"Processing", item, total), cancellationToken);
    }
  }

  public abstract record EditEvent
  {
    public sealed record Started(string Message, int Item, int Count) : EditEvent;
    public sealed record Milestone(string Message, int Item, int Count) : EditEvent;
    public sealed record Completed(string Message, int Item, int Count) : EditEvent;
    public sealed record Skipped(string Message, int Item, int Count) : EditEvent;
    public sealed record Error(string Message, int Item, int Count) : EditEvent;
  }

  public record RecordChange
  {
    public required string Field;
    public int Occ;
    public required string Change;
    public required string To;
  }

  public sealed record ConfirmRequest
  {
    public int RecordId;
    public string Prompt = "";
    public object? Payload = null;
    public List<RecordChange>? Changes;
  }

  public enum ConfirmationResult
  {
    /// <summary>
    /// Accept the change
    /// </summary>
    Accept,

    /// <summary>
    /// Do not accept the change
    /// </summary>
    Reject,

    /// <summary>
    /// Cancel the whole procedure
    /// </summary>
    Cancel
  }

  public interface IGlobalEditCallBacks
  {
    public Task ReportAsync(EditEvent ev, CancellationToken cancelationToken);
    public Task<ConfirmationResult> ConfirmAsync(ConfirmRequest request, CancellationToken cancelationToken);
  }

  public sealed class Callbacks(
     ObservableCollection<EditEvent> eventsSink,
     Func<ConfirmRequest, CancellationToken, Task<ConfirmationResult>> confirm) : IGlobalEditCallBacks
  {
    private readonly ObservableCollection<EditEvent> _events = eventsSink;
    private readonly Func<ConfirmRequest, CancellationToken, Task<ConfirmationResult>> _confirm = confirm;

    public Task ReportAsync(EditEvent ev, CancellationToken cancellationToken)
    {
      _events.Add(ev);
      return Task.CompletedTask;
    }

    public Task<ConfirmationResult> ConfirmAsync(
        ConfirmRequest confirmRequest,
        CancellationToken cancellationToken
    ) => _confirm(confirmRequest, cancellationToken);
  }
}
