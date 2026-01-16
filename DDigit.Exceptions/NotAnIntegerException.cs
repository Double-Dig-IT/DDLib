namespace DDigit.Exceptions;

public class NotAnIntegerException(string? name, string? value) : DDException($"Expected an integer search value for field '{name}', but got '{value}'")
{
  public string? Value { get; } = value;

  public string? Name { get; } = name;  
}