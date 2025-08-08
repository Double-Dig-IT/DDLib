namespace DDigit.Data;

public class IndexChanges : List<IndexRow>
{
  internal IndexChanges(IndexData index)
  {
    Index = index;
  }

  internal IndexData Index { get; }

  public override string ToString() => $"{Index.Name} ({Count})";

  internal IndexRow FindOrCreateRow(BooleanIndexRow row)
    => SelectRow(row, this.Cast<BooleanIndexRow>().
        FirstOrDefault(x => (string?)x.Term == (string?)row.Term && x.Id == row.Id));

  internal IndexRow FindOrCreateRow(IntegerIndexRow row)
    => SelectRow(row, this.Cast<IntegerIndexRow>().
        FirstOrDefault(x => (int?)x.Term == (int?)row.Term && x.Id == row.Id));

  /// <summary>
  /// The display key for alphanumeric values must match, the Term contains padded data
  /// </summary>
  /// <param name="row">The row to search for</param>
  /// <returns>An existing or a new row</returns>
  internal IndexRow FindOrCreateRow(AlphaNumericIndexRow row)
    => SelectRow(row, this.Cast<AlphaNumericIndexRow>().
       FirstOrDefault(x => (string?)x.DisplayTerm == (string?)row.DisplayTerm && x.Id == row.Id));
   

  internal IndexRow FindOrCreateRow(TermIndexRow row)
  {
    var result = SelectRow(row, this.
      Cast<TermIndexRow>().
        FirstOrDefault(x => (string?)x.Term == (string?)row.Term &&
                            x.Tag == row.Tag &&
                            x.Occ == row.Occ &&
                            x.Domain == row.Domain &&
                            x.Language == row.Language &&
                            x.Id == row.Id));
    return result;
  }

  internal IndexRow FindOrCreateRow(DateIndexRow row)
   => SelectRow(row, this.
      Cast<DateIndexRow>().
        FirstOrDefault(x => x.DisplayTerm == row.DisplayTerm && x.Id == row.Id));

  internal IndexRow FindOrCreateRow(IsoDateIndexRow row)
   => SelectRow(row, this.
      Cast<IsoDateIndexRow>().
        FirstOrDefault(x => x.Term == row.Term && x.Id == row.Id));

  private IndexRow SelectRow(IndexRow row, IndexRow? existing)
  {
    if (existing == null)
    {
      Add(row);
    }
    return existing ?? row;
  }
}

