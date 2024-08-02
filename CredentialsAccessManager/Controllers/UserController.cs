using AuthProvider;
using AuthProvider.Authentication.Authorizers;
using AuthProvider.AuthModelBinder;
using AuthProvider.CamInterface;
using AuthProvider.Utils;
using CredentialsAccessManager.Credentials;
using CredentialsAccessManager.Credentials.CredentialStore;
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
public class UserController(ILogger<UserController> logger, ICamInterface camInterface, ICredentialStore credentialStore) : ControllerBase
{
    private readonly ILogger Logger = logger;
    private readonly ICamInterface CamInterface = camInterface;
    private readonly ICredentialStore CredentialStore = credentialStore;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult CreateAccount([FromBody] UserCredentials userCredentials)
    {
        UserActionResult result = CredentialStore.CreateUser(userCredentials.Username, userCredentials.Password);
        
        // We could optionally log the user in here if we wanted but let's avoid that for now

        if (result.OperationSuccess)
            return Ok();
        else
        {
            return Conflict();
        }
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth<CredentialAuth>(Permission.LOGIN)]
    public void Login([FromAuth<AuthUserId>] Guid userId, [FromAuth<AuthType>] string type, [FromAuth<AuthUsername>] string username)
    {
        Logger.LogInformation($"FromAuth - userid: {userId}");
        Logger.LogInformation($"FromAuth - type: {type}");
        Logger.LogInformation($"FromAuth - username: {username}");
        /*
        Logger.LogInformation($"FromAuth - sessionId: {sessionId}");
        Logger.LogInformation($"FromAuth - apikey: {apikey}");
        Logger.LogInformation($"FromAuth - permission: {permission}");
        Logger.LogInformation($"FromAuth - service: {service}");*/
        
        string sessionId = CredentialStore.CreateNewSession(userId).Output;

        // Shouldn't fail here unless something went horribly wrong
        _ = CSRFUtils.TryGenerateCSRF(sessionId, out string csrf);

        Logger.LogInformation($"Session Id: {sessionId}\nCSRF: {csrf}");

        // Set the user's CSRF token
        CookieUtils.SetCookie(HttpContext.Response, CookieUtils.CSRF, csrf, CookieUtils.ScriptableCookieOptions);

        // Set the user's Session token
        CookieUtils.SetCookie(HttpContext.Response, CookieUtils.Session, sessionId, CookieUtils.SecureCookieOptions);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth<SessionAuth>]
    public IActionResult Logout([FromAuth<AuthSessionId>] string sessionId)
    {
        var result = CredentialStore.RevokeSessionBySessionId(sessionId);
        Console.WriteLine(result);
        // CookieUtils.RemoveCookie(HttpContext.Response, CookieUtils.Session);
        return Ok();
    }

    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth<StrictSessionAuth>]
    public IActionResult LogoutAll([FromAuth<AuthUserId>] Guid userId)
    {
        var result = CredentialStore.RevokeAllSessions(userId);
        Console.WriteLine(result);
        // CookieUtils.RemoveCookie(HttpContext.Response, CookieUtils.Session);
        return Ok();
    }
    /*
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth(Permission.WRITE_SELF)]
    public IActionResult UpdateUsername() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth(Permission.WRITE_SELF)]
    public IActionResult UpdatePassword() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth(Permission.WRITE_SELF)]
    public IActionResult UpdateUserInfo() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth(Permission.READ_SELF)]
    public IActionResult GetActiveSessions() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth(Permission.READ_SELF)]
    public IActionResult GetUserInfo() => throw new NotImplementedException();

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Auth(Permission.READ_SELF)]
    public IActionResult GetPermissions() => throw new NotImplementedException();
    */
}
