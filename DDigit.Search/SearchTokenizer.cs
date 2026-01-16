using System.DirectoryServices;

namespace DDigit.Search;

public class SearchTokenizer
{
  public SearchTokenizer(string searchStatement)
  {
    Statement = searchStatement;
    Length = Statement.Length;
    Pos = 0;
    GetToken(true);
  }

  public string GetToken(bool fieldName = false)
  {
    var result = new StringBuilder();
    char? quote = null;

    // skip leading whitespace
    while (!IsEndOfText && IsWhiteSpace(CurrentCharacter))
    {
      Pos++;
    }

    while (!IsEndOfText)
    {
      if (CurrentCharacter == quote)
      {
        Pos++;
        break;
      }

      if (CurrentCharacter == '/' && NextCharacter == '*')
      {
        IsComment = true;
        Pos += 2;
        continue;
      }

      if (CurrentCharacter == '*' && NextCharacter == '/')
      {
        IsComment = false;
        Pos += 2;
        continue;
      }

      if (IsComment)
      {
        Pos++;
        continue;
      }

      if (IsLiteral)
      {
        quote = CurrentCharacter;
        Pos++;
        continue;
      }

      if (quote != null)
      {
        // handle escaped ", ' and \ 
        if (CurrentCharacter == '\\')
        {
          switch (NextCharacter)
          {
            case '\"':
            case '\'':
            case '\\':
              Pos++;
              result.Append(CurrentCharacter);
              Pos++;
              break;

            default:
              result.Append(CurrentCharacter);
              Pos++;
              break;
          }
        }
        else
        {
          result.Append(CurrentCharacter);
          Pos++;
        }
        continue;
      }

      if (IsWhiteSpace(CurrentCharacter))
      {
        break;
      }

      if (fieldName)
      {
        if (IsSpecial(CurrentCharacter) && IsAllowedInFieldName(CurrentCharacter) && !IsSpecial(NextCharacter))
        {
          result.Append(CurrentCharacter);
          Pos++;
          continue;
        }
      }

      if (IsSpecial(CurrentCharacter))
      {
        if (result.Length == 0)
        {
          result.Append(CurrentCharacter);
          Pos++;
        }
        break;
      }

      result.Append(CurrentCharacter);
      Pos++;
    }
    Token = result.ToString();
    return Token;
  }

  internal bool Accept(string accept)
  {
    var accepted = string.Equals(Token, accept, StringComparison.CurrentCultureIgnoreCase);
    if (accepted)
    {
      Accepted = accept;
      GetToken();
    }
    return accepted;
  }

  internal void Expect(string expect)
  {
    if (string.Compare(Token, expect, true) != 0)
    {
      throw new UnexpectedTokenException(Token, expect);
    }
    GetToken();
  }

  internal int ExpectInteger()
  {
    if (!int.TryParse(Token, out int number))
    {
      throw new IntegerExpectedException(Token);
    }
    GetToken();
    return number;
  }

  internal int ExpectPositiveInteger(int max = 1)
  {
    if (!int.TryParse(Token, out int number) || number < max)
    {
      throw new PositiveIntegerExpectedException(Token);
    }
    GetToken();
    return number;
  }

  public override string? ToString() => Token;

  internal string Statement { get; private set; }
  internal int Length { get; private set; }
  internal int Pos { get; set; }
  internal char? NextCharacter => Pos + 1 < Length ? Statement[Pos + 1] : null;
  internal string? Token { get; private set; } = null;
  internal string Accepted { get; private set; } = string.Empty;
  public bool IsEndOfText => Pos >= Length;
  internal char? CurrentCharacter => !IsEndOfText ? Statement?[Pos] : null;
  private static bool IsWhiteSpace(char? c) => white.Contains(c);
  private static bool IsSpecial(char? c) => specials.Contains(c);
  private bool IsComment;
  private bool IsLiteral => CurrentCharacter == '\'' || CurrentCharacter == '"';
  private static bool IsAllowedInFieldName(char? c) => allowedInFieldName.Contains(c);


  private static readonly char?[] white = [' ', '\t', '\n', '\r'];
  private static readonly char?[] specials = ['(', ')', '[', ']', '=', '<', '>', ',', '+', '-'];
  private static readonly char?[] allowedInFieldName = ['-', '_', '.'];

}
