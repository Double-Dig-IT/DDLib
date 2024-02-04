namespace DDigit.Exceptions;

public class FieldIsNotRepeatedException(string tag, int occ) : DDException($"Field '{tag}' is not repeated")
{
  public string Tag
  {
    get;
  } = tag;

  public int Occ
  {
    get;
  } = occ;
}