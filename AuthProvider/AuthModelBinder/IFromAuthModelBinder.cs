using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AuthProvider.AuthModelBinder;

public interface IFromAuthModelBinder : IModelBinder
{
    public abstract static bool PreCheck(ControllerActionDescriptor action, ILogger logger, ParameterInfo parameter, Type AuthType);
}
