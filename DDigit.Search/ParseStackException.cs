namespace DDigit.Search;

internal class ParseStackException(string? message) : DDException($"Parse stack corruption: {message}")
{
}