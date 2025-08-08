
namespace DDigit.Repository;

public class AlphaNumericIndexRow : IndexRow
{
  public AlphaNumericIndexRow(IndexData indexData, string tag, object value, int id) : base(indexData, tag, 1, id)
  {
    if (value != null)
    {
      var text = value.ToString() ?? ""; 
      Term = KeyConversions.AlphaKeyValue(text, 10);
      DisplayTerm = value.ToString();
    }
  }
}