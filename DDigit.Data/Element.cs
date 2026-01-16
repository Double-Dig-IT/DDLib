

using System.Xml.Linq;

namespace DDigit.Data;

public class Element
{
  public Element(object? value, string language = "", bool invariant = false, bool? isLocal = null)
  {
    Values.Push(value);
    Language = language;
    Invariant = invariant;
    IsLocal = isLocal;
  }

  internal ConcurrentStack<object?> Values
  {
    get; 
  } = [];

  internal ConcurrentStack<object?> ReDoValues
  {
    get;
  } = [];

  public bool Invariant
  {
    get; set;
  }

  public string Language
  {
    get; set;
  }

  public bool? IsLocal
  {
    get; set;
  }

  public override string? ToString() => !Values.IsEmpty ? Values.TryPeek(out var result) ? result?.ToString() : null : null;

  public object? Value => !Values.IsEmpty ? Values.TryPeek(out var result) ? result : null : null;

  public int? IntValue
      => Values.TryPeek(out var result) ?
            result is string v ? int.TryParse(v, out var IntValue) ? 
              IntValue : null : (int?)result : null;

  internal void SetData(object? value) => Values.Push(value);

  internal void ReplaceData(string? value)
  {
    Values.TryPop(out _);
    Values.Push(value);
  }

  internal Element Clone() => new(Value, Language, Invariant);

  internal void Undo()
  {
    if (Values.Count > 1 && Values.TryPop(out var top))
    {
      ReDoValues.Push(top);
    }
  }

  internal void Redo()
  {
    if (!ReDoValues.IsEmpty && ReDoValues.TryPop(out var top))
    {
      Values.Push(top);
    }
  }
}