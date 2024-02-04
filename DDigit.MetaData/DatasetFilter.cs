namespace DDigit.MetaData;

public class DatasetFilter : Dictionary<string, DatasetData>
{
  public DatasetFilter(DatabaseData database, string[] datasets)
  {
    foreach (var dataset in datasets)
    {
      var datasetLimits = database.Datasets.FirstOrDefault(d => d.Name == dataset) ??
        throw new DatasetNotFoundException(dataset, database.Name);
      this[dataset] = datasetLimits;
    }
  }

  public bool Accepts(int id)
    => Values.Any
      (d => id >= d.LowerLimit && id <= d.UpperLimit);

  public override string ToString() => string.Join(", ", Keys);
}