namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public async Task<int?> SearchObjectAsync(DatabaseData objects, string barcodeObjectNumberOrId, CancellationToken cancellationToken)
  {
    async Task<int?> SearchByObjectNumber()
    {
      var statement = $"object_number = '{barcodeObjectNumberOrId}'";
      var result = await SearchAsync(objects, null, statement, cancellationToken: cancellationToken);
      return result?.FirstOrNull();
    }

    async Task<int?> SearchById()
    {
      if (!int.TryParse(barcodeObjectNumberOrId, out var id))
      {
        return null;
      }

      var statement = $"identifier = '{id}'";
      var result = await SearchAsync(objects, null, statement, cancellationToken: cancellationToken);
      return result?.FirstOrNull();
    }

    return await SearchByObjectNumber() ?? await SearchById();
  }
}
