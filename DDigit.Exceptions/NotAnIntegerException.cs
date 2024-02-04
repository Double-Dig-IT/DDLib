namespace DDigit.Exceptions;

public class NotAnIntegerException(string? value) : DDException($"Expected an integer search value, but got '{value}'")
{
  public string? Value { get; } = value;
}