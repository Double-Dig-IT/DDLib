namespace DDigit.Repository;

public class TermIndexRow : IndexRow
{
  /// <summary>
  /// Constructor for enum keys
  /// </summary>
  /// <param name="index">Metadata of the index</param>
  /// <param name="tag">Tag</param>
  /// <param name="occ">Occurrence</param>
  /// <param name="neutralValue">The neutral value for the enum data</param>
  /// <param name="id">record number</param>
  public TermIndexRow(IndexData index, string tag, int occ, object neutralValue, int id) : base (index, tag, occ, id)
  {
    Term = KeyConversions.DisplayTermValue(neutralValue, index.Length);
    DisplayTerm = KeyConversions.DisplayTermValue(neutralValue, index.Length);
  }

  public TermIndexRow(IndexData index, string? tag, int occ, object? value, string? domain, string language, int id) : base(index, tag, occ, id)
  {
    Term = KeyConversions.TermValue(value, index.Length);
    DisplayTerm = KeyConversions.DisplayTermValue(value, index.Length);
    StrippedTerm = KeyConversions.StrippedTermValue(value, index.Length);
    Domain = domain;
    Language = language;
  }

  public string? StrippedTerm
  {
    get; private set;
  } = null;

  public string? Domain
  {
    get; private set;
  } = null;

  public string? Language
  {
    get; private set;
  } = null;

  public override string ToString() => $"{Term} / {DisplayTerm} ({Domain}) [{Language}] {Count}";
}