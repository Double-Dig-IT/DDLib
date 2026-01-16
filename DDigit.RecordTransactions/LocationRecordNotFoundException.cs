namespace DDigit.RecordTransactions;

public class LocationRecordNotFoundException(int objectId, int locationId) :
  DDException($"Location record '{locationId}' cannot be found.")
{
  public int ObjectId { get; } = objectId;
  public int LocationId { get; } = locationId;
}
