using AuthProvider.RuntimePrecheck.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace AuthProvider.RuntimePrecheck.Context;
public class ActionPrecheckContext : AbstractPrecheckContext
{
    public ControllerPrecheckContext ParentControllerContext;
    public MethodInfo MethodInfo;
    public List<ParameterPrecheckContext> Parameters = [];

    public ActionPrecheckContext(ControllerPrecheckContext parent, ControllerActionDescriptor action) : base(action.MethodInfo.Name)
    {
        ParentControllerContext = parent;
        MethodInfo = action.MethodInfo;
        foreach (var item in MethodInfo.GetParameters())
        {
            Parameters.Add(new ParameterPrecheckContext(this, item));
        }
    }

    public override void RunPrecheck()
    {
        var attributes = MethodInfo.GetCustomAttributes();
        foreach (var attribute in attributes)
        {
            if (attribute is IActionPrecheckAttribute precheckAction)
            {
                Console.WriteLine(precheckAction.GetType());
                precheckAction.RunActionPrecheck(this);
            }
        }

        foreach (var parameter in Parameters)
        {
            parameter.RunPrecheck();
        }
    }

    public void PrettyPrint(ILogger logger)
    {
        logger.LogInformation($"Action: {MethodInfo.Name}");
        PrettyPrintIssues(logger);
        foreach (var parameter in Parameters)
        {
            parameter.PrettyPrint(logger);
        }
    }

    public new bool ShouldError()
    {
        if (base.ShouldError())
        {
            return true;
        }

        foreach (var parameter in Parameters)
        {
            if (parameter.ShouldError())
            {
                return true;
            }
        }

        return false;
    }
}
