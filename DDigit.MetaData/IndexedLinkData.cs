namespace DDigit.MetaData
{
  /// <summary>
  /// This represents indexed link tables, there are found in the wild since application 5.x and their purpose it to speed up link processing.
  /// </summary>
  /// <param name="field"></param>
  /// <param name="index"></param>
  public class IndexedLinkData(FieldData field, IndexData index)
  {
    public string SortField { get; private set; } = field.IndexedLinkSortField;
    public SortSequenceEnum SortOrder { get; private set; } = field.IndexedLinkSortOrder;
    public string? FormatString  { get; private set; } = field.IndexedLinkFormatString;
    public IndexData Index { get; private set; } = index;
  }
}