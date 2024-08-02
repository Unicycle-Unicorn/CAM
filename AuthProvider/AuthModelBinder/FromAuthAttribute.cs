using AuthProvider.RuntimePrecheck.Context;
using AuthProvider.RuntimePrecheck.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AuthProvider.AuthModelBinder;

public abstract class FromAuthAttribute(Type type) : ModelBinderAttribute(type)
{
    public static readonly BindingSource FromAuthBindingSource = new("Auth", "BindingSource_Auth", isGreedy: false, isFromRequest: false);

    public new BindingSource BindingSource => FromAuthBindingSource;

    public abstract bool PreCheck(ControllerActionDescriptor action, ILogger logger, HashSet<Type> availableFromAuthTypes, ParameterInfo parameterInfo);

}

[AttributeUsage(AttributeTargets.Parameter)]
public class FromAuthAttribute<T> : FromAuthAttribute, IParameterPrecheckAttribute, IBindingSourceMetadata where T : IFromAuthModelBinder
{
    public FromAuthAttribute() : base(typeof(T)) { }

    public override bool PreCheck(ControllerActionDescriptor action, ILogger logger, HashSet<Type> availableFromAuthTypes, ParameterInfo parameter)
    {
        bool shouldError = false;

        if (!availableFromAuthTypes.Contains(typeof(T)))
        {
            logger.LogError($"{action!.DisplayName} can not contain the parameter '[FromAuth<{typeof(T).Name}>] {parameter.ParameterType.Name} {parameter.Name}' as this type cannot be provided by the current [AuthAttribute]");
            shouldError = true;
        }

        if (T.PreCheck(action, logger, parameter, typeof(T)))
        {
            shouldError = true;
        }

        return shouldError;
    }

    public void RunParameterPrecheck(ParameterPrecheckContext context)
    {
        var attributes = context.ParentAction.MethodInfo.GetCustomAttributes<AuthAttribute>();

        HashSet<Type> availableFromAuthTypes = [];

        foreach (var attribute in attributes)
        {
            availableFromAuthTypes.UnionWith(attribute.GetFromAuthAvailableTypes());
        }

        if (!availableFromAuthTypes.Contains(typeof(T)))
        {
            context.AddFatal($"[FromAuth<{typeof(T).Name}>] is not provided by the current authentication scheme", "Change the authentication scheme or remove this parameter");
        }

        T.RunPrecheck(context, typeof(T));
    }
}