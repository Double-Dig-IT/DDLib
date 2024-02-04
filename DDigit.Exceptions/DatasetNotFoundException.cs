namespace DDigit.Exceptions;

public class DatasetNotFoundException(string? dataset, string? database) :
  DDException($"Dataset '{dataset}' not found in '{database}'")
{
  public string? Dataset
  {
    get;
  } = dataset;

  public string? Database
  {
    get;
  } = database;
}