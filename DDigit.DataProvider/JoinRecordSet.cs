namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public ResultSet JoinRecordSet(ResultSet left, BooleanOperator @operator, ResultSet right)
    => ResultSet.Join(left, @operator, right);

}
