using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace AuthProvider.RuntimePrecheck;

[AttributeUsage(AttributeTargets.All)]
public abstract class PrecheckMethodAttribute : Attribute
{
    public abstract bool PreCheck(ControllerActionDescriptor action, ILogger logger);
}
