namespace DDigit.DataProvider;

public partial class DDataProvider : IDataProvider
{
  public void SetUser(string folder, string userName, string? role, string? password)
  {
    bool modified = false;
    var applicationData = MetaDataCache.ReadApplication(folder, false) ??
      throw new ArgumentException("No application found in folder", nameof(folder));

    var user = applicationData.Users.FirstOrDefault((u) => u.Name == userName);
    if (user == null)
    {
      user = new UserData
      {
        Name = userName
      };
      applicationData.Users.Add(user);
      modified = true;
    }

    if (role != null)
    {
      user.Role = role;
      modified = true;
    }

    if (password != null)
    {
      user.SetPassword(password);
      modified = true;
    }

    if (modified)
    {
      applicationData.Save();
    }
  }
}
