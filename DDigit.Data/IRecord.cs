namespace DDigit.Data;

public interface IRecord
{
  public int Id { get; }

  public int RepFind(string tag, object value, string language = "");

  public Task DeleteAsync(CancellationToken cancellationToken);

  public Task WriteAsync(IDbConnection? connection, IDbTransaction? transaction, CancellationToken cancellationToken);
}
