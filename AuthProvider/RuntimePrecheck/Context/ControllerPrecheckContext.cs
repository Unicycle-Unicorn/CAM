using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AuthProvider.RuntimePrecheck.Context;
public class ControllerPrecheckContext : AbstractPrecheckContext
{
    public List<ActionPrecheckContext> Actions = [];
    public ApplicationPrecheckContext ParentApplicationContext;
    public TypeInfo ControllerInfo;

    public ControllerPrecheckContext(ApplicationPrecheckContext parent, List<ControllerActionDescriptor> controllerActionDescriptors) : base(controllerActionDescriptors.First().ControllerTypeInfo.Name)
    {
        ParentApplicationContext = parent;
        var first = controllerActionDescriptors.First();
        ControllerInfo = first.ControllerTypeInfo;

        foreach (var action in controllerActionDescriptors)
        {
            Actions.Add(new ActionPrecheckContext(this, action));
        }
    }

    public override void RunPrecheck()
    {
        foreach (var action in Actions)
        {
            action.RunPrecheck();
        }
    }

    public void PrettyPrint(ILogger logger)
    {
        logger.LogInformation($"Controller: {ControllerInfo.Name}");
        PrettyPrintIssues(logger);
        foreach (var action in Actions)
        {
            action.PrettyPrint(logger);
        }
    }

    public new bool ShouldError()
    {
        if (base.ShouldError())
        {
            return true;
        }

        foreach (var action in Actions)
        {
            if (action.ShouldError())
            {
                return true;
            }
        }

        return false;
    }
}
