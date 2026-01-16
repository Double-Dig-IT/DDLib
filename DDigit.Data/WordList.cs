namespace DDigit.Data;

internal class WordList
{
  internal static async Task<List<int>> GetWordNumbers(IDDRepository repository, List<string> words, string language,
                                                       SqlStateInfo sqlState)
  {
    static async Task<int> GetWordNumber(IDDRepository repository, LanguageWord word, SqlStateInfo sqlState)
    {
      object wordNo = wordlist.Get(word.ToString());
      if (wordNo != null)
      {
        return (int)wordNo;
      }

      int wordNumber = await repository.GetWordNumberAsync(word.Text, word.Language, sqlState);
      if (wordNumber != 0)
      {
        return wordNumber;
      }

      wordNumber = await repository.AddWord(word.Text, word.Language, sqlState);
      if (wordNumber != 0)
      {
        var cacheItem = new CacheItem(word.ToString(), wordNumber);
        var policy = new CacheItemPolicy();
        wordlist.Add(cacheItem, policy);
      }
      return wordNumber;
    }

    var result = new List<int>(words.Count);
    foreach (var word in words)
    {
      var wordNumber = await GetWordNumber(repository, new LanguageWord(language, word), sqlState);
      if (wordNumber > 0)
      {
        result.Add(wordNumber);
      }
    }
    return result;
  }

  private readonly static MemoryCache wordlist = new("wordlist");
}