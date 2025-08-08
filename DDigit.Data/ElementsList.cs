

namespace DDigit.Data;

public class ElementsList : ConcurrentDictionary<string, Element>
{
  internal void AddRange(ElementsList elements)
  {
    foreach (var element in elements)
    {
      this[element.Key] = element.Value.Clone();
    }
  }

  internal string? GetMultiLingualData(string language)
  {
    if (TryGetValue(Languages.GetDataLanguage(language), out var element))
    {
      return element.ToString();
    }
    element = Values.FirstOrDefault(element => element.Invariant);
    if (element != null)
    { 
      return element.ToString();
    }
    if (TryGetValue("", out element))
    {
      return element.ToString();
    }
    return null;
  }
}
