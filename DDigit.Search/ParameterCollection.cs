namespace DDigit.Search;

public class ParameterCollection : Dictionary<string, object>
{
  /// <summary>
  /// Save a prameter on the parameter list
  /// </summary>
  /// <param name="value">Object to save</param>
  /// <returns>Name of the parameter</returns>
  public string Save(object value) => Save("term", value);

  /// <summary>
  /// Save a prameter on the parameter list
  /// </summary>
  /// <param name="parameter">Nsme of the parameter</param>
  /// <param name="value">Object to save</param>
  /// <returns>Name of the parameter</returns>
  public string Save(string parameter, object value)
  {
    var parameterName = $"@{parameter}{parameterIndex++}";
    this[parameterName] = value;
    return parameterName;
  }

  /// <summary>
  /// Produces a parameter list for testing in SSMS
  /// </summary>
  /// <returns></returns>
  public string ToSql()
  {
    static string SqlType(object value)
        => value switch
        {
          string s => $"nvarchar ({s.Length}) = '{value}'",
          int => $"int = {(int)value}",
          _ => $"nvarchar({value.ToString()!.Length}) = '{value}'"
        };

    var parts = new List<string>(Count);
    foreach ((string name, object value) in this)
    {
      parts.Add($"declare {name} {SqlType(value)}");
    }
    return string.Join("\n", parts);
  }

  // (@sortterm0, 0), (@sortterm1, 1), etc...
  public string Ranks() => 
    string.Join(", ", Keys.
      Where(key => key.StartsWith("@sortTerm")).
        Select((key, index) => $"({key}, {index})"));

  private int parameterIndex = 0;
}
