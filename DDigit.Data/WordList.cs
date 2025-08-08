namespace DDigit.Data;

internal class WordList
{
  internal static async Task<List<int>> GetWordNumbers(IDbConnection connection, IDbTransaction transaction,
                                                       IDDRepository repository, List<string> words, string language,
                                                       CancellationToken cancellationToken)
  {
    var result = new List<int>(words.Count);
    foreach (var word in words)
    {
      var wordNumber = await GetWordNumber(connection, transaction, repository, new LanguageWord(language, word), cancellationToken);
      if (wordNumber > 0)
      {
        result.Add(wordNumber);
      }
    }
    return result;
  }

  private static async Task<int> GetWordNumber(IDbConnection connection, IDbTransaction transaction, IDDRepository repository,
                                               LanguageWord word, CancellationToken cancellationToken)
  {
    object wordNo = wordlist.Get(word.ToString());
    if (wordNo != null)
    {
      return (int)wordNo;
    }

    int wordNumber = await repository.GetWordNumber(connection, transaction, word.Text, word.Language);
    if (wordNumber != 0)
    {
      return wordNumber;
    }

    wordNumber = await repository.AddWord(connection, transaction, word.Text, word.Language, cancellationToken);
    if (wordNumber != 0)
    {
      var cacheItem = new CacheItem(word.ToString(), wordNumber);
      var policy = new CacheItemPolicy();
      wordlist.Add(cacheItem, policy);
    }
    return wordNumber;
  }

  private readonly static MemoryCache wordlist = new("wordlist");
}