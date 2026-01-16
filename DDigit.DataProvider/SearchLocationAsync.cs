namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<int?> SearchLocationAsync(DatabaseData locations, string barcodeLocationCodeOrId, CancellationToken cancellationToken)
  {
    async Task<int?> SearchByBarcode()
    {
      var statement = $"barcode = '{barcodeLocationCodeOrId}'";
      var result = await SearchAsync(locations, null, statement, cancellationToken: cancellationToken);
      return result?.FirstOrNull();
    }

    async Task<int?> SearchByLocationCode()
    {
      var statement = $"name path '{barcodeLocationCodeOrId}'";
      var result = await SearchAsync(locations, null, statement, cancellationToken: cancellationToken);
      return result?.FirstOrNull();
    }

    async Task<int?> SearchById()
    {
      if (!int.TryParse(barcodeLocationCodeOrId, out var id))
      {
        return null;
      }

      var statement = $"identifier = '{id}'";
      var result = await SearchAsync(locations, null, statement, cancellationToken: cancellationToken);
      return result?.FirstOrNull();
    }

    return await SearchByBarcode() ?? await SearchByLocationCode() ?? await SearchById();
  }
}
