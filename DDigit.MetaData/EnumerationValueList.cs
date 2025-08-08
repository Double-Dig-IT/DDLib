
namespace DDigit.MetaData;

public class EnumerationValueList : List<EnumerationValueData>
{
  internal IEnumerable<EnumerationValueData> GetValues(string? value, int languageNo)
  {
    if (value == null)
    {
      return [];
    }
    if (value.EndsWith('*'))
    {
      return this.Where(v => v.NeutralValue != null &&
                                    (value == null || (v.Texts[languageNo].Text != null &&
                                    v.Texts[languageNo].Text!.StartsWith(value[..(value.Length - 1)],
                                    StringComparison.CurrentCultureIgnoreCase))));
    }
    else
    {
      return this.Where(v => v.NeutralValue != null &&
                                    (value == null || (v.Texts[languageNo].Text != null &&
                                    v.Texts[languageNo].Text!.Equals(value, StringComparison.CurrentCultureIgnoreCase))));

    }
  }
}