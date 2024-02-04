namespace DDigit.Data;

internal static class Extensions
{
  internal static void AddField(this JsonObject record, string fieldName, object value, SerializeOptions? options = null)
  {
    var fields = options?.Fields;
    if (fields == null || fields.Contains(fieldName) || fields.Contains("*"))
    {
      if (value is string s)
      {
        record[fieldName] = s;
        return;
      }
      if (value is int i)
      {
        record[fieldName] = i;
        return;
      }
      if (value is DateTime d)
      {
        record[fieldName] = d;
        return;
      }
      throw new DataException($"Data type {value.GetType().Name} is not supported (yet)");
    }
  }
}
