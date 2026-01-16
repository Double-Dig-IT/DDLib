namespace DDigit.Data;

public interface IRecord
{
  public int Id { get; }

  public int RepFind(string tag, object value, string language = "");

  public Task DeleteAsync(CancellationToken cancellationToken);

  public Task WriteAsync(SqlStateInfo? sqlState, RecordWriteOptionsFlag? writeOptions);
}
