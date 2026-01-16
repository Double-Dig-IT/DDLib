namespace DDigit.Search;

public class ResultSet
{
    public ResultSet(IEnumerable<int> prirefs) : this()
    {
        Ids = [.. prirefs];
        Hits = Ids.Count;
    }

    public ResultSet()
    {
    }

    public ResultSet(string? workingDirectory, string? database)
    {
        WorkingDirectory = workingDirectory;
        Database = database;
    }

    public ResultSet(DatabaseData databaseData)
    {
        WorkingDirectory = Path.GetDirectoryName(databaseData.FileName);
        Database = databaseData.Name;
    }

    public ResultSet(string? workingDirectory, string? database, List<int> set) : this(workingDirectory, database)
    {
        Ids = set;
        Hits = Ids.Count;
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
    } = new(500);

    public List<object> Keys { get; set; } = [];

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

    public int? FirstOrNull()
    {
        return Ids.Count > 0 ? Ids[0] : null;
    }

    public void Add(int id, DatasetFilter? filter, HashSet<int>? previousResults)
    {
        if ((filter == null || filter.Accepts(id)) &&
            (previousResults == null || previousResults.Contains(id)))
        {
            Ids.Add(id);
        }
    }


    //public int Count => Ids.Count;

    public int Hits { get; set; } = 0;

    public int DistinctHits { get; private set; } = 0;

    public override string ToString() => $"{Database} ({WorkingDirectory}) : {Hits:n0} records";

    public ResultSet Randomize(SearchTree searchTree)
    {
        if (!searchTree.SampleSize.HasValue)
        {
            return this;
        }

        var random = searchTree.Seed.HasValue ? new Random(searchTree.Seed.Value) : new Random();

        var sampleSize = Math.Min(searchTree.SampleSize.Value, Ids.Count);
        List<int> resultIds = new(sampleSize);
        while (resultIds.Count < sampleSize)
        {
            int index = random.Next(0, Ids.Count);
            var id = Ids.ElementAt(index);
            resultIds.Add(id);
            Ids.Remove(id);
        }
        return new ResultSet(WorkingDirectory, Database, resultIds);
    }

    public ResultSet Limit(SearchTree searchTree)
    {
        DistinctHits = Ids.Distinct().Count();

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

    public void AddId(int id) => Ids.Add(id);

    public void AddKey(object o) => Keys.Add(o);
}
