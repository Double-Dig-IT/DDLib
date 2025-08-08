namespace DDigit.Search;

public class ResultSet
{
  public ResultSet(IEnumerable<int> prirefs) : this()
  {
    Ids = [.. prirefs];
  }

  public ResultSet()
  {
    Count = Ids.Count;
  }

  public ResultSet(string? workingDirectory, string? database)
  {
    WorkingDirectory = workingDirectory;
    Database = database;
  }

  public ResultSet(DatabaseData databaseData)
  {
    WorkingDirectory = Path.GetDirectoryName(databaseData.PhysicalPath);
    Database = databaseData.Name;
  }

  public ResultSet(string? workingDirectory, string? database, List<int> set) : this(workingDirectory, database)
  {
    Ids = set;
    Count = Ids.Count;
  }

  public string? Database
  {
    get; set;
  }

  public string? WorkingDirectory
  {
    get; set;
  }

  public List<int> Ids
  {
    get; set;
  } = new(500_000);

  public int this[int index] => Ids[index];

  public static ResultSet Join(ResultSet left, BooleanOperator @operator, ResultSet right)
   => @operator switch
   {
     BooleanOperator.And => left * right,
     BooleanOperator.Or => left + right,
     BooleanOperator.Not => left - right,
     _ => throw new NotSupportedException($"Boolean operator {@operator}"),
   };

  public static ResultSet operator *(ResultSet left, ResultSet right) =>
    new(left.WorkingDirectory, left.Database, [.. left.Ids.Intersect(right.Ids)]);

  public static ResultSet operator +(ResultSet left, ResultSet right) =>
    new(left.WorkingDirectory, left.Database, [.. left.Ids.Union(right.Ids)]);

  public static ResultSet operator -(ResultSet left, ResultSet right)
    => new(left.WorkingDirectory, left.Database, [.. left.Ids.Except(right.Ids)]);

  public void Add(int id, DatasetFilter? filter, HashSet<int>? previousResults)
  {
    if ((filter == null || filter.Accepts(id)) &&
        (previousResults == null || previousResults.Contains(id)))
    {
      Ids.Add(id);
    }
  }


  public int Count { get; set; }

  public override string ToString() => $"{Database} ({WorkingDirectory}) : {Count:n0} records";

  public ResultSet Randomize(SearchTree searchTree)
  {
    if (searchTree.SampleSize == 0)
    {
      return this;
    }

    var random = searchTree.Seed != 0 ? new Random(searchTree.Seed) : new Random();
    var result = new ResultSet(WorkingDirectory, Database);

    var sampleSize = Math.Min(searchTree.SampleSize, Ids.Count);
    while (result.Ids.Count < sampleSize)
    {
      int index = random.Next(0, Ids.Count);
      var id = Ids.ElementAt(index);
      result.Ids.Add(id);
      Ids.Remove(id);
    }
    result.Count = result.Ids.Count;
    return result;
  }

  public ResultSet Limit(SearchTree searchTree)
  {
    if (searchTree.StartFrom.HasValue && searchTree.StartFrom > 1)
    {
      Ids = [.. Ids.Skip(searchTree.StartFrom.Value - 1)];
    }
    if (searchTree.Limit.HasValue)
    {
      Ids = [.. Ids.Take(searchTree.Limit.Value)];
    }
    return this;
  }

}
