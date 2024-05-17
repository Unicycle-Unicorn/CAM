using CredentialsAccessManager.Models;

namespace CredentialsAccessManager.User;

public class UserData
{
    public string Username;
    public string Password;
    public Dictionary<string, HashSet<string>> Permissions = new()
        {
            { "CAM", [Permission.LOGIN, Permission.READ_SELF, Permission.WRITE_SELF] }
        };

    public UserInformation UserInformation;

    public UserData(string username, string password, UserInformation userInformation)
    {
        Username = username;
        Password = password;
        UserInformation = userInformation;
    }
}
