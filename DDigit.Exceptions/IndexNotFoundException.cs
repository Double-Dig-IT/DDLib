namespace DDigit.Exceptions;

public class IndexNotFoundException(string? fieldName) :
  DDException($"No preferred index for field '{fieldName}'")
{
}