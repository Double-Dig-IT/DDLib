using System.Text;

namespace DDigit.Data;

public static class Extensions
{
  internal static void AddId(this JsonObject record, string database, int id, SerializeOptions? options)
  {
    if (options?.JsonLD == true &&
        options.Databases is { } databases && databases.Contains(database))
    {
      var url = options.API ?? "https://n2t.net";
      var ark = $"ark:/{options.NAAN}/{database}/{id}";
      record["@id"] = $"{url}/{ark}";
    }
    record["record.identifier"] = id;
  }

  internal static void AddIIIF(this JsonNode node, string? element, SerializeOptions? options)
  {
    if (options?.IIIF is { } endpoint && !string.IsNullOrEmpty(element))
    {
      node["IIIF.url"] = $"{endpoint}/{element}/full/max/0/default.jpg";
    }
  }

  internal static void Add(this JsonNode node, string name, JsonObject addition)
  {
    if (node is JsonArray array)
    {
      array.Add(addition);
    }
    else
    {
      node[name] = addition;
    }
  }

  internal static void Add(this JsonNode node, string name, object? value)
  {
    if (value != null)
    {
      if (node is JsonArray array)
      {
        array.Add(value?.ToString());
      }
      else
      {
        node[name] = value?.ToString();
      }
    }
  }

  internal static void AddField(this JsonObject record, string fieldName, object value, SerializeOptions? options = null)
  {
    var fields = options?.Fields;
    if (fields == null || fields.Contains("*") || fields.Contains(fieldName))
    {
      if (value is string s)
      {
        record[fieldName] = s;
        return;
      }
      if (value is int i)
      {
        record[fieldName] = i;
        return;
      }
      if (value is DateTime d)
      {
        record[fieldName] = d;
        return;
      }
      throw new DataException($"Data type {value.GetType().Name} is not supported (yet)");
    }
  }

  public static string EscapeQuotes(this string term)
  {
    var escaped = new StringBuilder();
    foreach (var ch in term)
    {
      if (ch == '\\' || ch == '\'' || ch == '\"')
      {
        escaped.Append('\\');
      }
      escaped.Append(ch);
    }
    return escaped.ToString();
  }
}
