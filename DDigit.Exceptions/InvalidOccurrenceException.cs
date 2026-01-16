namespace DDigit.Exceptions;

public class InvalidOccurrenceException: DDException
{
  public InvalidOccurrenceException(int occ) : base($"Invalid occurrence '{occ}' found.")
  {
    Occ = occ;
  }

  public InvalidOccurrenceException(string? name, int occ) : base($"Invalid occurrence '{occ}' found for field '{name}'.")
  {
    Name = name;
    Occ = occ;
  }

  public int Occ
  {
    get; private set;
  }

  public string? Name { get; }
}