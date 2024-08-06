using AuthProvider;
using AuthProvider.Authentication.Authorizers;
using AuthProvider.AuthModelBinder;
using AuthProvider.CamInterface;
using CredentialsAccessManager.Credentials.CredentialStore;
using Microsoft.AspNetCore.Mvc;

namespace CredentialsAccessManager.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class InternalController(ICredentialStore credentialStore) : ControllerBase
{
    private readonly ICredentialStore CredentialStore = credentialStore;

    [HttpPost("{service}")]
    public IActionResult Authenticate([FromBody] AuthorizationRequest authRequest, [FromRoute] string service)
    {
        switch (authRequest.AuthRequestType)
        {
            case AuthRequestType.ApiKey:
                if (authRequest.ApiKey is null) break;
                return Ok(CredentialStore.AuthenticateApiKey(authRequest.ApiKey));
            case AuthRequestType.Credentials:
                if ((authRequest.Username is null) || (authRequest.Password is null)) break;
                return Ok(CredentialStore.AuthenticateCredentials(authRequest.Username, authRequest.Password));
            case AuthRequestType.Session:
                if (authRequest.SessionId is null) break;
                return Ok(CredentialStore.AuthenticateSession(authRequest.SessionId));
            case AuthRequestType.StrictSession:
                if ((authRequest.SessionId is null) || (authRequest.Password is null)) break;
                return Ok(CredentialStore.AuthenticateStrictSession(authRequest.SessionId, authRequest.Password));
        }

        return Ok(AuthorizationResult.Failed());
    }

    [HttpPost("{service}/{permission}")]
    public IActionResult Authorize([FromBody] AuthorizationRequest authRequest, [FromRoute] string service, [FromRoute] string permission)
    {
        var permissionGroup = (service, permission);

        switch (authRequest.AuthRequestType)
        {
            case AuthRequestType.ApiKey:
                if (authRequest.ApiKey is null) break;
                return Ok(CredentialStore.AuthorizeApiKey(authRequest.ApiKey, permissionGroup));
            case AuthRequestType.Credentials:
                if ((authRequest.Username is null) || (authRequest.Password is null)) break;
                return Ok(CredentialStore.AuthorizeCredentials(authRequest.Username, authRequest.Password, permissionGroup));
            case AuthRequestType.Session:
                if (authRequest.SessionId is null) break;
                return Ok(CredentialStore.AuthorizeSession(authRequest.SessionId, permissionGroup));
            case AuthRequestType.StrictSession:
                if ((authRequest.SessionId is null) || (authRequest.Password is null)) break;
                return Ok(CredentialStore.AuthorizeStrictSession(authRequest.SessionId, authRequest.Password, permissionGroup));
        }

        return Ok(AuthorizationResult.Failed());
    }

    /*
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetUsernameFromUserId([FromBody] Guid userId)
    {
        string? username = UserStore.GetUsernameFromUserId(userId);

        return username == null ? NotFound() : Ok(username);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public IActionResult GetUserIdFromUsername([FromBody] string username)
    {
        Guid? userId = UserStore.GetUserIdFromUsername(username);

        return userId == null ? NotFound() : Ok(userId);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Authorize([FromBody] PermissionCheck permissionCheck)
    {
        if (SessionStore.AuthenticateSession(permissionCheck.Session.UserId, permissionCheck.Session.SessionId))
        {
            if (permissionCheck.Permission == null || permissionCheck.Service == null)
            {
                return Ok();
            }

            if (UserStore.HasPermission(permissionCheck.Session.UserId, permissionCheck.Service, permissionCheck.Permission)) {
                return Ok();
            }

            return Forbid();
        }
        return Unauthorized();
    }*/
}
