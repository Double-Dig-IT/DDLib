namespace DDigit.Exceptions;

public class RecordSetNotFoundException(string database, int set) : 
  DDException($"Record set not found in database '{database}' with set number {set}.")
{
  public string Database { get; private set; } = database;
  public int Set { get; private set; } = set;
}