namespace DDigit.Classes;

public class RecordLock
{
  public string? Database
  {
    get;
    set;
  }

  public int Id
  {
    get;
    set;
  }

  public string? User
  {
    get;
    set;
  }

  public DateTime LockTime
  {
    get;
    set;
  }

  public string? Info
  {
    get;
    set;
  }
}
