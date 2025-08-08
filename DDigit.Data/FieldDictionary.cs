namespace DDigit.Data;

internal class FieldDictionary : ConcurrentDictionary<string, OccurrenceList>
{
  internal FieldDictionary Clone()
  {
    FieldDictionary fieldDictionary = [];
    foreach (var (tag, occurrenceList) in this)
    {
      fieldDictionary[tag] = occurrenceList.Clone();
    }
    return fieldDictionary;
  }


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

  internal string? GetData(string domainTag, int occ) => TryGetValue(domainTag, out var occurrenceList) ? occurrenceList.GetData(occ) : null;

  internal int RepCount(string domainTag) => TryGetValue(domainTag, out var occurrences) ? occurrences.Count : 0;
}
