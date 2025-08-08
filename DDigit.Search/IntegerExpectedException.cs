namespace DDigit.Search;

[Serializable]
public class IntegerExpectedException(string? token) : DDException($"Integer expected, but got '{token}'.")
{
}