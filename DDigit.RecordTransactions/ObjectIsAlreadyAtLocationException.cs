namespace DDigit.RecordTransactions;

public class ObjectIsAlreadyAtLocationException(int objectId, int locationId) :
  DDException($"Object '{objectId}' is already at location '{locationId}'.")
{
  public int ObjectId { get; } = objectId;
  public int LocationId { get; } = locationId;
}
