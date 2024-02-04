namespace DDigit.Exceptions;

[Serializable]
public class InvalidMetaDataException(string? message) : DDException(message)
{
}
