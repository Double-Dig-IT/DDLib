namespace DDigit.Search;

[Serializable]
public class NumberExpectedException(string token) : DDException($"Number expected, but got '{token}'.")
{
}