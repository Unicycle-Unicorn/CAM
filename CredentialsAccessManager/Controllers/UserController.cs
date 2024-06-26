using AuthProvider;
using CredentialsAccessManager.Models;
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

void CreateAccount(string username, string password, UserData userData): password doesn't meet requirements (422), duplicate username (409)
void Login(string username, string password): incorrect pass or username (401)
<> void Logout()
<> void LogoutSession(sessionId) // we're gonna have to move to hashes to support this in a secure way
<> void LogoutAll()

<ws> void UpdateUsername(string password, string newUsername): incorrect password (401), duplicate username (409)
<ws> void UpdatePassword(string oldPassword, string newPassword): incorrect password (401), new password doesn't meet requirements (422)
<ws> void UpdateUserInfo(string password, UserData newUserData): incorrect password (401)
<rs> [ActiveSession] GetActiveSessions()
<rs> UserData GetUserInfo()
<rs> {Permission} GetPermissions()
*/

namespace CredentialsAccessManager.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IUserStore UserStore;
    private readonly ISessionStore SessionStore;

    public UserController(IUserStore userStore, ISessionStore sessionStore)
    {
        UserStore = userStore;
        SessionStore = sessionStore;
    }

    [HttpGet]
    public IActionResult TestGet()
    {
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult CreateAccount([FromBody] UserCredentials userCredentials) => UserStore.CreateUser(userCredentials.Username, userCredentials.Password, new UserInformation()) ? Ok() : Conflict();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Login([FromBody] UserCredentials userCredentials)
    {
        if (UserStore.AttemptLogin(userCredentials.Username, userCredentials.Password, out Guid userId))
        {
            if (UserStore.HasPermission(userId, RegistrationService.Service, Permission.LOGIN))
            {
                SessionCredentials session = SessionStore.CreateNewSession(userId);

                SessionCookieUtils.AttachSession(Response, session);

                return Ok();
            }

            return Forbid();
        }

        return Unauthorized();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [CustomAuthorization]
    public IActionResult Logout()
    {
        SessionCredentials session = GetSession();
        SessionStore.RevokeSession(session.UserId, session.SessionId);
        SessionCookieUtils.RemoveSession(Response);
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [CustomAuthorization]
    public IActionResult LogoutAll()
    {
        SessionCredentials session = GetSession();
        SessionStore.RevokeAllSessions(session.UserId);
        SessionCookieUtils.RemoveSession(Response);
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [CustomAuthorization(Permission.WRITE_SELF)]
    public IActionResult UpdateUsername() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [CustomAuthorization(Permission.WRITE_SELF)]
    public IActionResult UpdatePassword() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [CustomAuthorization(Permission.WRITE_SELF)]
    public IActionResult UpdateUserInfo() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [CustomAuthorization(Permission.READ_SELF)]
    public IActionResult GetActiveSessions() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [CustomAuthorization(Permission.READ_SELF)]
    public IActionResult GetUserInfo() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [CustomAuthorization(Permission.READ_SELF)]
    public IActionResult GetPermissions() => throw new NotImplementedException();

    [NonAction]
    public SessionCredentials GetSession() => HttpContext.Features.Get<SessionCredentials>();
}
