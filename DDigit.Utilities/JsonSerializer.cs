namespace DDigit.Utilities;

public static class JsonSerializer
{
  public static string Serialize(object obj, JsonSerializerOptions? options = null)
    => System.Text.Json.JsonSerializer.Serialize(obj, options ?? writeOptions);

  public static void Serialize(Stream utf8json, object obj, JsonSerializerOptions? options = null)
    => System.Text.Json.JsonSerializer.Serialize(utf8json, obj, options ?? writeOptions);

  public static object? DeSerialize(string json, Type type, JsonSerializerOptions? options = null)
    => System.Text.Json.JsonSerializer.Deserialize(json, type, options ?? readOptions);

  public static TValue? DeSerialize<TValue>(string json, JsonSerializerOptions? options = null)
    => System.Text.Json.JsonSerializer.Deserialize<TValue>(json, options ?? readOptions);

  public static object? DeSerialize(Stream utf8json, Type type, JsonSerializerOptions? options = null)
    => System.Text.Json.JsonSerializer.Deserialize(utf8json, type, options ?? readOptions);

  public static TValue? DeSerialize<TValue>(Stream utf8json, JsonSerializerOptions? options = null)
    => System.Text.Json.JsonSerializer.Deserialize<TValue>(utf8json, options ?? readOptions);

  public static JsonElement? GetProperty(JsonElement jsonElement, string name)
    => jsonElement.TryGetProperty(name, out var property) ? property : null;

  public static T? GetProperty<T>(JsonElement jsonElement, string name)
  {
    T? result = default;
    if (jsonElement.TryGetProperty(name, out var property))
    {
      result = property.Deserialize<T>();
    }
    return result;
  }

  private readonly static JsonSerializerOptions writeOptions = new()
  {
    WriteIndented = true,
    IgnoreReadOnlyFields = true,
    IgnoreReadOnlyProperties = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    Converters =
    {
      new JsonStringEnumConverter()
    }
  };

  private readonly static JsonSerializerOptions readOptions = new()
  {
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    Converters =
    {
      new JsonStringEnumConverter()
    }
  };
}
