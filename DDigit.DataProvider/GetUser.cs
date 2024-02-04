namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public IEnumerable<UserData> GetUser(string folder, string? userName, string? role)
    => GetApplication(folder).Users.Where(user =>
       MatchList.Match(
       [
          new MatchPair(userName, user.Name),
          new MatchPair(role, user.Role)
       ]));

}
