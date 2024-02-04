

namespace DDigit.Data;

public class OccurrenceList : List<Occurrence>
{
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

  internal Occurrence FindOrCreate(int occ)
  {
    if (occ < 1)
    {
      throw new InvalidOccurrenceException(occ);
    }
    int index = occ - 1;
    while (Count < occ)
    {
      Add(new Occurrence());
    }
    return this[index];
  }

  internal Occurrence InsertOrCreate(int occ)
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
        Add(new Occurrence());
      }
    }
    else
    {
      Insert(index, new Occurrence());
    }
    return this[index];
  }
}
