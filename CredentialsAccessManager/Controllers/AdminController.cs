using CredentialsAccessManager.Session;
using CredentialsAccessManager.User;
using Microsoft.AspNetCore.Mvc;

/*
<> stands for authentication, string inside is the Permission nessesary
possible permissions - readself, writeself, readadmin, writeadmin
other permissions - login, 

All endpoints can result in:
401 - Session does not exist or has expired
403 - Session valid but permissions not met
404 - user not found

<writeadmin> void GrantPermission(string username, string Service, string Permission)
<writeadmin> void RevokePermission(string username, string Service, string Permission)
<readadmin> {Permission} GetPermissions(string username)
<readadmin> [ActiveSession] GetActiveSessions(string username)
<readadmin> UserData GetUserInfo(string username)
*/

namespace CredentialsAccessManager.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class AdminController : ControllerBase
{
    private readonly IUserStore UserStore;
    private readonly ISessionStore SessionStore;

    public AdminController(IUserStore userStore, ISessionStore sessionStore)
    {
        UserStore = userStore;
        SessionStore = sessionStore;
    }


}
