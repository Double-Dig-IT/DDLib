using System.Net.WebSockets;

namespace DDigit.Data;

public class Occurrence
{
  public Occurrence()
  {
  }

  public Occurrence(Element data)
  {
    var language = data.Language;
    Elements[language] = data;
    if (data.Invariant && !string.IsNullOrWhiteSpace(language))
    {
      Elements[""] = Elements[language];
    }
  }

  internal void Set(string language, string? value, bool invariant)
  {
    if (Elements.TryGetValue(language, out var fieldElement))
    {
      fieldElement.SetData(value);
    }
    else
    {
      Elements[language] = new Element(value, language, invariant)
      {
        Modified = true
      };
    }
  }

  public Element? this[string language]
  {
    get
    {
      if (!string.IsNullOrWhiteSpace(language))
      {
        if (Elements.TryGetValue(language, out var element))
        {
          return element;
        }
        var result = Elements.Values.FirstOrDefault(e => e.Invariant);
        if (result == null)
        {
          Elements.TryGetValue("", out result);
        }
        return result;
      }
      else
      {
        return Elements.TryGetValue(language, out var element) ? element : null;
      }
    }
  }

  public readonly Dictionary<string, Element> Elements = [];

  public override string? ToString() => Elements[""].ToString();
}