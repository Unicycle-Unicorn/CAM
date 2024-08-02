using AuthProvider.CamInterface;
using CredentialsAccessManager.Credentials.CredentialStore;
using Microsoft.AspNetCore.Mvc;

namespace CredentialsAccessManager.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class InternalController(ICamInterface camInterface, ICredentialStore credentialStore) : ControllerBase
{
    private readonly ICamInterface CamInterface = camInterface;
    private readonly ICredentialStore CredentialStore = credentialStore;

    [HttpGet]
    public IActionResult TestGet()
    {
        throw new NotImplementedException();
        return Ok();
    }

    [HttpGet]
    public IActionResult TestGet2()
    {
        return Ok();
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
