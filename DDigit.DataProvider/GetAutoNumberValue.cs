namespace DDigit.DataProvider;

public partial class DDataProvider
{
  public Task<string> GetAutoNumberValue(FieldData fieldData, SqlStateInfo sqlState)
    => Repository.GetAutoNumberValue(fieldData, sqlState);
}
