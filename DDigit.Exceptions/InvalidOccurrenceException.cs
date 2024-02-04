namespace DDigit.Exceptions;

public class InvalidOccurrenceException(int occ) :
  DDException($"Invalid occurrence '{occ}' found.")
{
  public int Occ
  {
    get;
  } = occ;
}