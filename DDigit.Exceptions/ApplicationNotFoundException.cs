namespace DDigit.Exceptions;

public class ApplicationNotFoundException(string folder) : DDException($"Application not found in folder '{folder}'")
{
  public string Folder
  {
    get;
  } = folder;
}
