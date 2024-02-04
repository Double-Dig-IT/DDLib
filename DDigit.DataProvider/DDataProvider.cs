namespace DDigit.DataProvider;

public partial class DDataProvider(IDDRepository repository) : IDataProvider
{
  public IDDRepository Repository { get; private set; } = repository;

  public event EventHandler<MilestoneEventArgs>? MilestoneReached;

  public int Milestone { get; set; } = 1000;
}
