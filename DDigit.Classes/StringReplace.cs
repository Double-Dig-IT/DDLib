using System.Text;

namespace DDigit.Classes;

public static class StringReplace
{
  public enum MatchScope { Substring, WholeWord, WholeString }

  public sealed class ReplaceOptions
  {
    public bool CaseSensitive { get; set; } = false;
    public MatchScope Scope { get; set; } = MatchScope.Substring;
    public bool UseWildcards { get; set; } = false; // '*' = wildcard for 0..n characters
    public bool CultureInvariant { get; set; } = true;
  }

  public static string Replace(string input, string search, string replacement, ReplaceOptions options, out int replacements)
  {
    if (input is null)
    {
      throw new ArgumentNullException(nameof(input));
    }

    if (search is null)
    {
      throw new ArgumentNullException(nameof(search));
    }

    if (replacement is null)
    {
      replacement = string.Empty;
    }

    replacements = 0;

    if (search.Length == 0)
      return input;

    var compare = options.CaseSensitive
        ? StringComparison.Ordinal
        : (options.CultureInvariant ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCultureIgnoreCase);

    // === WHOLE STRING MATCH ===
    if (options.Scope == MatchScope.WholeString)
    {
      bool isMatch = false;

      if (options.UseWildcards)
      {
        // Match the entire input using wildcard pattern
        int wildcardMatch = WildcardMatchLength(input.AsSpan(), search, compare);
        isMatch = (wildcardMatch == input.Length);
      }
      else
      {
        isMatch = string.Equals(input, search, compare);
      }

      if (isMatch)
      {
        replacements = 1;
        return replacement;
      }

      return input;
    }

    // === SUBSTRING / WHOLEWORD REPLACEMENT ===
    var text = input.AsSpan();
    var sb = new StringBuilder(input.Length);
    int i = 0;

    while (i < text.Length)
    {
      int matchLen = TryMatchAt(text, i, search, options, compare);
      if (matchLen > 0)
      {
        // If we only want whole words, verify boundaries
        if (options.Scope == MatchScope.WholeWord && !IsWordBoundaryOK(text, i, i + matchLen))
        {
          sb.Append(text[i]);
          i++;
          continue;
        }

        sb.Append(replacement);
        replacements++;
        i += matchLen;
      }
      else
      {
        sb.Append(text[i]);
        i++;
      }
    }

    return sb.ToString();
  }

  public static string Replace(string input, string search, string replacement, ReplaceOptions opt)
      => Replace(input, search, replacement, opt, out _);

  // Attempt to match the pattern at a specific index
  private static int TryMatchAt(ReadOnlySpan<char> text, int start, string pattern,
                                ReplaceOptions options, StringComparison comparison)
  {
    if (options.UseWildcards)
    {
      // Use wildcard matching
      int length = WildcardMatchLength(text[start..], pattern, comparison);
      return length > 0 ? length : 0;
    }
    else
    {
      // Simple direct comparison
      if (text[start..].StartsWith(pattern, comparison))
      {
        return pattern.Length;
      }
      return 0;
    }
  }

  private static bool IsWordBoundaryOK(ReadOnlySpan<char> text, int start, int end)
  {
    bool leftOk = (start == 0) || !char.IsLetterOrDigit(text[start - 1]);
    bool rightOk = (end == text.Length) || !char.IsLetterOrDigit(text[end]);
    return leftOk && rightOk;
  }

  // Returns the length of the shortest match (0..n) if wildcard pattern fits at the start
  // Returns -1 if no match
  private static int WildcardMatchLength(ReadOnlySpan<char> text, string pattern, StringComparison comparison)
  {
    // If no '*' in the pattern, use direct prefix match
    if (!pattern.Contains('*'))
    {
      return text.StartsWith(pattern, comparison) ? pattern.Length : -1;
    }

    var parts = pattern.Split('*');
    int pos = 0;

    // Match first part at the beginning
    if (parts.Length > 0 && parts[0].Length > 0)
    {
      if (!text[pos..].StartsWith(parts[0], comparison)) return -1;
      pos += parts[0].Length;
    }

    // Match intermediate parts
    for (int i = 1; i < parts.Length - 1; i++)
    {
      string segment = parts[i];
      if (segment.Length == 0) continue;

      int idx = text[pos..].ToString().IndexOf(segment, comparison);
      if (idx < 0) return -1;

      pos += idx + segment.Length;
    }

    // Match final segment (if any)
    string last = parts[^1];
    if (last.Length == 0)
    {
      // trailing '*': match can end here
      return pos;
    }

    if (text[pos..].StartsWith(last, comparison))
    {
      return pos + last.Length;
    }

    return -1;
  }
}
