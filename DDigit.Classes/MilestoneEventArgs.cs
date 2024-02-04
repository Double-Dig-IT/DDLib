namespace DDigit.Classes;

public class MilestoneEventArgs(int milestone, int total, int hits = 0) : EventArgs
{
  public int Milestone
  {
    get; private set;
  } = milestone;

  public int Total
  {
    get; private set;
  } = total;

  public int Hits
  {
    get; private set;
  } = hits;
}