namespace DDigit.Data;

public class Element
{
  public Element(object? value, string language = "", bool invariant = false)
  {
    Values.Push(value);
    Language = language;
    Invariant = invariant;
  }

  internal Stack<object?> Values
  {
    get; set;
  } = new();


  public object? NeutralValue
  { 
    get; 
    set;
  }


  public bool Invariant
  {
    get; set;
  }

  public string Language
  {
    get; set;
  }

  public bool Modified
  {
    get; set;
  }

  public object? InsertValue => Values.Count > 0 ? Values.First() : null;

  public object? DeleteValue => Values.Count > 0 ? Values.Last() : null;

  public override string? ToString() => Values.Count > 0 ? Values.Peek()?.ToString() : null;

  public object? Value => Values.Count > 0 ? Values.Peek() : null;

  internal void SetData(string? value)
  {
    Values.Push(value);
    Modified = true;
  }
}