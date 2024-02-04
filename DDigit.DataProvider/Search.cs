

namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  /// <summary>
  /// Run a search statement
  /// </summary>
  /// <param name="folder">The folder in which the database metadata is stored</param>
  /// <param name="database">The database to query</param>
  /// <param name="statement">The statement to executer</param>
  /// <param name="previousResults">Previous results (from a PowerShell pipeline)</param>
  /// <returns></returns>
  public async Task<ResultSet?> Search(string folder, string database, string[]? datasets, string statement, ResultSet? previousResults = null, int milestone = 1000)
  {
    var databaseData = MetaDataCache.ReadDatabase(folder, database, false);
    if (databaseData == null)
    {
      return null;
    }

    var datasetFilter = datasets != null ? new DatasetFilter(databaseData, datasets) : null;
    var searchTree = DDSearch.Parse(databaseData, statement);
    return searchTree != null ? await Search(databaseData, datasetFilter, searchTree, previousResults, milestone) : null;
  }


  private async Task<ResultSet> Search(DatabaseData databaseData, DatasetFilter? datasetFilter, SearchNode node, ResultSet? previousResults, int milestone)
  {
    ResultSet? result = null;
    if (node is SearchTreeNode searchTreeNode)
    {
      result = JoinRecordSet(await Search(databaseData, datasetFilter, searchTreeNode.Left, previousResults, milestone),
                           searchTreeNode.Operator,
                           await Search(databaseData, datasetFilter, searchTreeNode.Right, previousResults, milestone));
    }
    else if (node is SearchTreeLeaf leaf)
    {
      result = await FindRecordSet(databaseData, leaf.Field!, leaf.Language, leaf.Operator, leaf.Value.ToString(), datasetFilter, previousResults, milestone, MilestoneReached);
    }
    else if (node is SearchSetLeaf searchSetLeaf)
    {
      result = await FindRecordSet(databaseData, searchSetLeaf.SetId, datasetFilter, previousResults);
    }
    else
    {
      throw new DDException($"Unexpected search node: {node}");
    }
    return result.Randomize(node.SampleSize, node.Seed, node.Unique);
  }
}
