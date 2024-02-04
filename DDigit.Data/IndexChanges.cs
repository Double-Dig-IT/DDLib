namespace DDigit.Data;

public class IndexChanges
{
  internal IndexChanges(IndexData index)
  {
    Index = index;
  }

  public List<IndexRow> Modifications
  {
    get;
    private set;
  } = [];

  internal IndexData Index
  {
    get;
  }

  public override string ToString() => $"{Index.Name} ({Modifications.Count})";

  internal void DeleteKey(string table, int key, int id) =>
    FindRow(table, key, id).Count--;

  internal void DeleteKey(string table, string term, string displayTerm, string? domain, string language, int id) =>
    FindRow(table, term, displayTerm, domain, language, id).Count--;

  internal void InsertKey(string table, int key, int id) =>
    FindRow(table, key, id).Count++;

  internal void InsertKey(string table, string term, string displayTerm, string? domain, string language, int id) =>
   FindRow(table, term, displayTerm, domain, language, id).Count++;

  private IntegerIndexRow FindRow(string table, int wordNumber, int id)
  {
    var row = new IntegerIndexRow(table, wordNumber, id);
    var existingRow = Modifications.Cast<IntegerIndexRow>().FirstOrDefault(x => x.Key == row.Key);
    if (existingRow != null)
    {
      row = existingRow;
    }
    else
    {
      Modifications.Add(row);
    }
    return row;
  }

  private TermIndexRow FindRow(string table, string term, string displayTerm, string? domain,
                  string language, int id)
  {
    var row = new TermIndexRow(table, term, displayTerm, domain, language, id);
    var existingRow = Modifications.Cast<TermIndexRow>().FirstOrDefault(x => x.Term == row.Term &&
                                                        x.Domain == row.Domain &&
                                                        x.Language == row.Language &&
                                                        x.Id == row.Id);
    if (existingRow != null)
    {
      row = existingRow;
    }
    else
    {
      Modifications.Add(row);
    }
    return row;
  }
}

