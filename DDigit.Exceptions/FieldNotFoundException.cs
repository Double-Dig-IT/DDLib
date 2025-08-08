namespace DDigit.Exceptions;

public class FieldNotFoundException : DDException
{
  public FieldNotFoundException(string field, string? database) :
    base($"Field '{field}' not found in '{database}'")
  {
    Tag = field;
    Database = database;
  }

  public FieldNotFoundException(string tag, int occ, string? database)
    : base($"Field '{tag}' not found in '{database}'")
  {
    Tag = tag;
    Occ = occ;
    Database = database;
  }

  public FieldNotFoundException(string tag, int occ, string language, string? database)
    : base($"Field '{tag}' not found in '{database}'")
  {
    Tag = tag;
    Occ = occ;
    Language = language;
    Database = database;
  }

  public string Tag
  {
    get;
  }

  public int Occ
  {
    get;
  }

  public string? Language
  {
    get;
  }

  public string? Database
  {
    get;
  }

}

