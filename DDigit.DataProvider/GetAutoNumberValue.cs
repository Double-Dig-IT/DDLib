
namespace DDigit.DataProvider;

public partial class DDataProvider
{
  public Task<string> GetAutoNumberValue(IDbConnection connection, IDbTransaction transaction, 
                                   FieldData fieldData, CancellationToken cancellationToken)
    => Repository.GetAutoNumberValue(connection, transaction, fieldData, cancellationToken);
}
