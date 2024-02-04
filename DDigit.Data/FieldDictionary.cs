

namespace DDigit.Data;

internal class FieldDictionary : Dictionary<string, OccurrenceList>
{
  internal FieldDictionary Clone() => (FieldDictionary)MemberwiseClone();

  internal OccurrenceList FindOrCreateOccurrenceList(string? tag)
  {
    if (tag == null)
    {
      throw new NullReferenceException(nameof(tag));
    }
    if (!TryGetValue(tag, out var occurrences))
    {
      occurrences = this[tag] = [];
    }
    return occurrences;
  }
}
