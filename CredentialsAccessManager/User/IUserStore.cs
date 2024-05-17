using CredentialsAccessManager.Models;

namespace CredentialsAccessManager.User;

public interface IUserStore
{
    public bool AttemptLogin(string username, string password, out Guid userId);
    public bool CreateUser(string username, string password, UserInformation userInformation);
    /*
    public bool UpdateUsername(Guid userId, string newUsername);
    public void UpdatePassword(Guid userId, string newPassword);
    
    public bool UpdateUserInformation(Guid userId, UserInformation newUserInformation);
    */
    public Guid? GetUserIdFromUsername(string username);
    public string? GetUsernameFromUserId(Guid userId);
    public bool HasPermission(Guid userId, string service, string permission);
}
