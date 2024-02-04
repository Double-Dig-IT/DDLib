namespace DDigit.Search;

internal class SearchValueExpectedException : DDException
{
  public SearchValueExpectedException() : base("Expected a search value")
  {
  }
}