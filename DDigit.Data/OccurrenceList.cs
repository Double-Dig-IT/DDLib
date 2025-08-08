


namespace DDigit.Data;

public class OccurrenceList : List<Occurrence>
{
  internal OccurrenceList Clone()
  {
    OccurrenceList occurrenceList = [];
    foreach (var occurrence in this)
    {
      occurrenceList.Add(occurrence.Clone());
    }
    return occurrenceList;
  }

  internal void Delete(int occ)
  {
    if (occ < 1)
    {
      throw new InvalidOccurrenceException(occ);
    }
    int index = occ - 1;
    if (index < Count)
    {
      RemoveAt(index);
    }
  }

  internal Occurrence FindOrCreate(OccurrenceDataTypeEnum dataType, int occ)
  {
    if (occ < 1)
    {
      throw new InvalidOccurrenceException(occ);
    }
    int index = occ - 1;
    while (Count < occ)
    {
      Add(new Occurrence(dataType));
    }
    return this[index];
  }

  internal Occurrence InsertOrCreate(OccurrenceDataTypeEnum dataType, int occ)
  {
    if (occ < 1)
    {
      throw new InvalidOccurrenceException(occ);
    }
    int index = occ - 1;
    if (Count < occ)
    {
      while (Count < occ)
      {
        Add(new Occurrence(dataType));
      }
    }
    else
    {
      Insert(index, new Occurrence(dataType));
    }
    return this[index];
  }

  internal string? GetData(int occ) => occ > 0 && occ <= Count ? this[occ - 1].GetData() : null;
}
