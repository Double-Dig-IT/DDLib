namespace DDigit.Exceptions;

public class AutoCompleteException(string folder, string database, string[] fields, string? value, Exception ex) :
  Exception($"AutoComplete Exception: folder='{folder}' database='{database}' fields='{(string.Join(',', fields))}' value='{value}' {ex}")
{
  public string Folder { get; } = folder;

  public string Database { get; } = database;

  public string[] Fields { get; } = fields;

  public string? Value { get; } = value;
}
