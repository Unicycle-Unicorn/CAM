using AuthProvider.AuthModelBinder;
using AuthProvider.RuntimePrecheck.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AuthProvider.RuntimePrecheck;

public class RuntimePrechecker
{
    private static ILogger Logger;

    public static void RunPrecheck(WebApplication app)
    {
        Logger = app.Logger;
        var prechecker = new ApplicationPrecheckContext(app, "Application");
        prechecker.RunPrecheck();
        //prechecker.PrettyPrint(app.Logger);
        Console.WriteLine(prechecker);
        if (prechecker.ShouldError())
        {
            throw new RuntimePrecheckException();
        }

        /*
        bool shouldFail = false;
        Logger.LogInformation("Starting precheck");
        
        IEnumerable<ControllerActionDescriptor> items = app.Services.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items.Cast<ControllerActionDescriptor>();
        Logger.LogCritical(items.First().ControllerTypeInfo.FullName);
        foreach (var action in items)
        {
            //var action = item as ControllerActionDescriptor;
            Logger.LogInformation($"Starting precheck on {action!.DisplayName}");
            if (RunActionPrecheck(action!))
            {
                Logger.LogError($"Failed precheck on {action!.DisplayName}");
                shouldFail = true;
            } else
            {
                Logger.LogInformation($"Successfully completed precheck on {action!.DisplayName}");
            }
            
        }

        if (shouldFail)
        {
            Logger.LogError("Precheck failed");
            throw new RuntimePrecheckException();
        }

        Logger.LogInformation("Precheck successfully completed");
        */
    }

    private static bool RunActionPrecheck(ControllerActionDescriptor action)
    {
        bool shouldFail = false;

        // Check to make sure no two AuthAttributes are on the same action
        var authAttrs = action!.MethodInfo.GetCustomAttributes<AuthAttribute>();
        if (authAttrs.Count() > 1)
        {
            shouldFail = true;
            Logger.LogError($"{action!.DisplayName} should only have one [AuthAttribute]");
        }

        // Check all attributes requesting to be checked
        var methodPrecheckAttributes = action!.MethodInfo.GetCustomAttributes<PrecheckMethodAttribute>();
        foreach (var item in methodPrecheckAttributes)
        {
            if (item.PreCheck(action, Logger))
            {
                shouldFail = true;
            }
        }

        HashSet<Type> availableFromAuthTypes = [];
        foreach (var authAttr in authAttrs)
        {
            availableFromAuthTypes.UnionWith(authAttr.GetFromAuthAvailableTypes());
        }

        // Check all from auth parameter attributes requesting to be checked
        foreach (var parameter in action!.MethodInfo.GetParameters())
        {
            var attributes = parameter.GetCustomAttributes<FromAuthAttribute>();
            if (attributes.Count() > 1) {
                shouldFail = true;
                Logger.LogError($"{action!.DisplayName} parameter '{parameter.ParameterType.Name} {parameter.Name}' should only have one [FromAuth] attribute");
            }

            foreach (var attribute in attributes)
            {
                if (attribute.PreCheck(action, Logger, availableFromAuthTypes, parameter)) {
                    shouldFail = true;
                }
            }
        }
        return shouldFail;
    }
}
