namespace DDigit.Exceptions;

public class FieldIsNotRepeatedException(string? fieldNameOrTag, int occ) : DDException($"Field '{fieldNameOrTag}' is not repeated")
{
  public string? FieldNameOrTag
  {
    get;
  } = fieldNameOrTag;

  public int Occ
  {
    get;
  } = occ;
}