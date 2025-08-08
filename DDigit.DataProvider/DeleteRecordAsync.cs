namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public Task DeleteRecordAsync(Record record, CancellationToken cancellationToken)
      => DeleteRecord(record.Database ?? throw new NullReferenceException(nameof(record.Database)), record.Id, cancellationToken);
}