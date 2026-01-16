namespace DDigit.Classes;

public class SqlStateInfo
{
  public static SqlStateInfo Default { get; set; } = new();
  public IDbConnection? Connection { get; set; }
  public IDbTransaction? Transaction { get; set; }
  public CancellationToken CancellationToken { get; set; } = default;
  public bool ProcessingReverseLinks { get; set; }
}
