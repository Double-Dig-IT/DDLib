namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<FormData> GetForm(string folder, string? formName = "*") =>
   MetaDataCache.FindForms(folder, formName);
}
