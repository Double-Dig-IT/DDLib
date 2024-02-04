namespace DDigit.Classes;

public class ResultSet
{
  public ResultSet(IEnumerable<int> prirefs)
  {
    foreach (var priref in prirefs)
    {
      Ids.Add(priref);
    }
  }

  public ResultSet()
  {
  }

  internal ResultSet(string? workingDirectory, string? database)
  {
    WorkingDirectory = workingDirectory;
    Database = database;
  }

  public ResultSet(DatabaseData databaseData)
  {
    WorkingDirectory = Path.GetDirectoryName(databaseData.PhysicalPath);
    Database = databaseData.Name;
  }

  public string? Database
  {
    get; set;
  }

  public string? WorkingDirectory
  {
    get; set;
  }

  public HashSet<int> Ids
  {
    get; set;
  } = new(50000);

  public int this[int index] => Ids.ElementAt(index);

  public static ResultSet Join(ResultSet left, BooleanOperator @operator, ResultSet right)
   => @operator switch
   {
     BooleanOperator.And => left * right,
     BooleanOperator.Or => left + right,
     BooleanOperator.Not => left - right,
     _ => throw new NotSupportedException($"Boolean operator {@operator}"),
   };

  public static ResultSet operator * (ResultSet left, ResultSet right)
 
    => new()
    {
       WorkingDirectory = left.WorkingDirectory,
       Database = left.Database,
       Ids = new HashSet<int>(left.Ids.Intersect(right.Ids)),
    };

  public static ResultSet operator + (ResultSet left, ResultSet right)
    => new()
    {
      WorkingDirectory = left.WorkingDirectory,
      Database = left.Database,
      Ids = new HashSet<int>(left.Ids.Union(right.Ids)),
    };

  public static ResultSet operator - (ResultSet left, ResultSet right)
    => new()
    {
      WorkingDirectory = left.WorkingDirectory,
      Database = left.Database,
      Ids = new HashSet<int>(left.Ids.Except(right.Ids)),
    };
   
  public void Add(int id, DatasetFilter? filter, ResultSet? previousResults)
  {
    if ((filter == null || filter.Accepts(id)) &&
          (previousResults == null || previousResults.Includes(id)))
    {
      Ids.Add(id);
    }
  }

  private bool Includes(int id) => Ids.Contains(id);

  public List<int> Slice(int start, int limit)
  {
    var result = new List<int>();
    int i = 0;
    int written = 0;
    foreach (var hit in Ids)
    {
      i++;
      if (start > 0)
      {
        if (i < start)
        {
          continue;
        }
      }
      result.Add(hit);
      written++;
      if (limit > 0 && written == limit)
      {
        break;
      }
    }
    return result;
  }

  public int Count => Ids.Count;

  public override string ToString() => $"{Database} ({WorkingDirectory}) : {Count}";

  public ResultSet Randomize(int? sampleSize, int? seed, bool? unique)
  {
    if (sampleSize == null)
    {
      return this;
    }

    var random = seed != null ? new Random(seed.Value) : new Random();
    var result = new ResultSet(WorkingDirectory, Database);
    sampleSize = Math.Min(sampleSize.Value, Ids.Count);
    for (int i = 0; i < sampleSize.Value; i++)
    {
      result.Ids.Add(Ids.ElementAt(random.Next(0, Ids.Count)));
    }
    return result;
  }
}
