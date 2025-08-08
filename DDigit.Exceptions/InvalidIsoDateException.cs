namespace DDigit.Exceptions;

public class InvalidIsoDateException(string? text) : DDException($"Invalid ISO date: '{text}'")
{
}
