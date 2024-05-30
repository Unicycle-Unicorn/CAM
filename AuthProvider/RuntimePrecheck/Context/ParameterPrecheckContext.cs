using AuthProvider.RuntimePrecheck.Interfaces;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AuthProvider.RuntimePrecheck.Context;
public class ParameterPrecheckContext : AbstractPrecheckContext
{
    public ActionPrecheckContext ParentAction;
    public ParameterInfo ParameterInfo;

    public ParameterPrecheckContext(ActionPrecheckContext actionPrecheckContext, ParameterInfo parameter) : base(parameter.Name!)
    {
        ParentAction = actionPrecheckContext;
        ParameterInfo = parameter;
    }

    public override void RunPrecheck()
    {
        var attributes = ParameterInfo.GetCustomAttributes();
        foreach (var attribute in attributes)
        {
            if (attribute is IParameterPrecheckAttribute precheckParameter)
            {
                precheckParameter.RunParameterPrecheck(this);
            }
        }
    }

    public void PrettyPrint(ILogger logger)
    {
        logger.LogInformation($"Parameter: {ParameterInfo.Name}");
        PrettyPrintIssues(logger);
    }
}
