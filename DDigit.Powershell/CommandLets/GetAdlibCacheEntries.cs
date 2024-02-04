namespace DDigit.PowerShell;

/// <summary>
/// Get a list of cache entries
/// </summary>
[Cmdlet(VerbsCommon.Get, AdlibNouns.CacheEntries)]
[OutputType(typeof(string))]
public class GetAdlibCacheEntries : DDCmdlet
{
  /// <summary>
  /// Do the work
  /// </summary>
  protected override void ProcessRecord()
  {
    var cacheEntries = provider.GetCacheEntries();
    if (SessionState != null)
    {
      foreach (var key in cacheEntries)
      {
        WriteObject(key);
      }
    }
  }
}
