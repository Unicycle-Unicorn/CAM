using AuthProvider;
using AuthProvider.Authentication.Authorizers;
using AuthProvider.AuthModelBinder;
using Microsoft.AspNetCore.Mvc;

namespace CredentialsAccessManager.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TestController : ControllerBase
{
    

    [HttpGet]
    public IActionResult TestKnownError()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [AuthAttribute<SessionAuth>]
    public IActionResult TestSessioned([FromAuth<AuthSessionId>] string sessionId)
    {
        Console.WriteLine($"Session Id: {sessionId}");
        return Ok();
    }

    [HttpGet]
    [AuthAttribute<SessionAuth, CredentialAuth>]
    public IActionResult TestSessionedOrCredential([FromAuth<AuthSessionId>] string sessionId)
    {
        Console.WriteLine($"Session Id: {sessionId}");
        return Ok();
    }

    [HttpGet]
    [AuthAttribute<CredentialAuth>]
    public IActionResult TestCredentials()
    {
        return Ok();
    }

    [HttpGet]
    [AuthAttribute<SessionAuth>("nofound")]
    public IActionResult TestSessionForbid()
    {
        return Ok();
    }
}
