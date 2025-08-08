namespace DDigit.Data;

public class Occurrence
{
  public Occurrence(OccurrenceDataTypeEnum dataType)
  {
    DataType = dataType;
  }

  public Occurrence(OccurrenceDataTypeEnum dataType, Element element)
  {
    DataType = dataType;
    Elements[element.Language] = element;
  }

  internal void Set(OccurrenceDataTypeEnum dataType, string language, object? value, bool invariant)
  {
    DataType = dataType;
    if (Elements.TryGetValue(language, out var fieldElement))
    {
      fieldElement.SetData(value);
    }
    else
    {
      Elements[language] = new Element(value, language, invariant);
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

  public OccurrenceDataTypeEnum DataType { get; set; }

  public object? NeutralValue
  {
    get;
    set;
  }

  public readonly ElementsList Elements = [];

  public Element? Invariant => Elements.Values.FirstOrDefault(element => element.Invariant);

  /// <summary>
  /// A link ref should always be in the elements dictionary with the language set to ""
  /// 23-05-23 BDD made this a bit more robust in case of incorrect data in the database.
  /// </summary>
  public int? LinkId
  {
    get
    {
      int? result = null;
      // link ref is always in the elements dictionary with the language set to ""
      if (Elements.TryGetValue("", out var element))
      {
        // if the element is not null and the value is not null
        if (element != null && element.Value != null)
        {
          // if the value is a string and can be parsed to an int
          if (element.Value is int v)
          {
            result = v;
          }
        }
      }
      return result;
    }
  }

  public override string? ToString() => Elements[""].ToString();

  internal string? GetData(string language = "")
    => DataType switch
    {
      OccurrenceDataTypeEnum.Id
      or OccurrenceDataTypeEnum.Standard => Elements.TryGetValue("", out var element) ? element.ToString() : null,
      OccurrenceDataTypeEnum.Enumeration => language == "" ? NeutralValue?.ToString() : Elements.GetMultiLingualData(language),
      OccurrenceDataTypeEnum.Multilingual => Elements.GetMultiLingualData(language),
      OccurrenceDataTypeEnum.LinkRef => LinkId?.ToString(),
      _ => throw new DDException($"Illegal data type {DataType}"),
    };


  internal Occurrence Clone()
  {
    Occurrence occurrence = new(DataType);
    foreach (var (language, element) in Elements)
    {
      occurrence.Elements[language] = element.Clone();
    }
    return occurrence;
  }
}