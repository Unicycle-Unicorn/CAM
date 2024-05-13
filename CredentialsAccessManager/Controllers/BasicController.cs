using Microsoft.AspNetCore.Mvc;

namespace CredentialsAccessManager.Controllers;
[ApiController]
[Route("[controller]")]
public class BasicController : ControllerBase
{

    public BasicController()
    {
        
    }

    [HttpGet(Name = "Get")]
    public IActionResult Get()
    {
        return Ok();
    }
}
