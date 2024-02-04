namespace DDigit.Data;

public class TermIndexRow(string table, string term, string displayTerm, string? domain, string language, int id) :
  IndexRow(table, id)
{
  public string Term
  {
    get; private set;
  } = term;

  public string DisplayTerm
  {
    get; private set;
  } = displayTerm;

  public string? Domain
  {
    get; private set;
  } = domain;

  public string Language
  {
    get; private set;
  } = language;

  public override string ToString() => $"{Term} / {DisplayTerm} ({Domain}) [{Language}] {Count}";
}