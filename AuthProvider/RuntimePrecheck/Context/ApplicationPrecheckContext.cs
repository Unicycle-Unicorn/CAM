using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text;

namespace AuthProvider.RuntimePrecheck.Context;

public class ApplicationPrecheckContext : AbstractPrecheckContext
{
    public List<ControllerPrecheckContext> Controllers = [];

    public ApplicationPrecheckContext(WebApplication app, string name) : base(name)
    {
        // Retrieve all of the actions in the app
        IEnumerable<ControllerActionDescriptor> controllerActions = app.Services.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items.Cast<ControllerActionDescriptor>();

        // Split each action into groups using thier parent controller
        Dictionary<TypeInfo, List<ControllerActionDescriptor>> tempControllers = [];
        foreach (var action in controllerActions)
        {
            var controller = action.ControllerTypeInfo;
            if (tempControllers.TryGetValue(controller, out var descriptors))
            {
                descriptors.Add(action);
            }
            else
            {
                tempControllers.Add(controller, [action]);
            }
        }

        foreach (var item in tempControllers)
        {
            Controllers.Add(new ControllerPrecheckContext(this, item.Value));
        }
    }

    public override void RunPrecheck()
    {
        foreach (var controller in Controllers)
        {
            controller.RunPrecheck();
        }
    }

    public void PrettyPrint(ILogger logger)
    {
        logger.LogInformation($"Starting application precheck");
        logger.LogInformation($"Application: {Name}");
        PrettyPrintIssues(logger);
        foreach (var controller in Controllers)
        {
            controller.PrettyPrint(logger);
        }
        logger.LogInformation($"Completed application precheck");
    }

    public new bool ShouldError()
    {
        if (base.ShouldError())
        {
            return true;
        }

        foreach (var controller in Controllers)
        {
            if (controller.ShouldError())
            {
                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Starting application precheck");
        AppendName("Application", Name, sb, 0);
        AppendIssues(Issues, sb, 0);
        foreach (var controller in Controllers)
        {
            AppendName("Controller", controller.Name, sb, 1);
            AppendIssues(controller.Issues, sb, 1);
            foreach (var action in controller.Actions)
            {
                AppendName("Action", action.Name, sb, 2);
                AppendIssues(action.Issues, sb, 2);
                foreach (var parameter in action.Parameters)
                {
                    AppendName("Parameter", parameter.Name, sb, 3);
                    AppendIssues(parameter.Issues, sb, 3);
                }
            }
        }
        sb.AppendLine("Application precheck completed");

        return sb.ToString();
    }

    private void AppendName(string type, string name, StringBuilder sb, int indents)
    {
        indents *= 4;
        sb.Append(' ', indents).Append(type).Append(": ").AppendLine(name);
    }

    private void AppendIssues(List<RuntimePrecheckIssue> issues, StringBuilder sb, int indents)
    {
        indents *= 4;
        foreach (var issue in issues)
        {
            sb.Append(' ', indents).Append("  + ").Append(issue.Severity).Append(' ');
            sb.Append(issue.Issue);

            if (issue.SuggestedFix != null)
            {
                sb.Append(" (").Append(issue.SuggestedFix).Append(')');
            }
            sb.AppendLine();
        }
    }
}
