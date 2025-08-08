namespace DDigit.Exceptions;

public class DDException(string? message, Exception? ex = null) : Exception(message, ex)
{
}