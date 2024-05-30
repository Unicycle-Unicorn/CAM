using AuthProvider.RuntimePrecheck.Context;
using AuthProvider.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AuthProvider.AuthModelBinder;

public abstract class FromAuthModelBinder<T> : IFromAuthModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext.ModelType == typeof(T))
        {
            T result = GetModel(bindingContext.HttpContext);
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            throw new Exception($"Failed to bind {typeof(T).Name} to {bindingContext.ModelType.Name} for {bindingContext.FieldName} due to a type mismatch occuring in request method: '{bindingContext.ActionContext.ActionDescriptor.DisplayName}'. [FromAuth<{GetType().Name}>] should be bound on a {typeof(T).Name} instead of {bindingContext.ModelType.Name}");
        }

        return Task.CompletedTask;
    }

    public T GetModel(HttpContext context)
    {
        if (ItemUtils.TryGet(context, GetType(), out object? value) && value != null)
        {
            return (T)value;
        }

        return GetDefault();
    }

    public abstract T GetDefault();
    public static bool PreCheck(ControllerActionDescriptor action, ILogger logger, ParameterInfo parameter, Type AuthType)
    {
        if (parameter.ParameterType != typeof(T))
        {
            logger.LogError($"FromAuth<{AuthType.Name}>] returns {typeof(T).Name}, however the method parameter is '{parameter.ParameterType.Name} {parameter.Name}' ");
            return true;
        }

        return false;
    }

    public static void RunPrecheck(ParameterPrecheckContext context, Type authType)
    {
        if (context.ParameterInfo.ParameterType != typeof(T))
        {
            context.AddFatal($"Parameter type mismatch. [FromAuth<{authType.Name}>] returns {typeof(T).Name} however the method parameter is expecting {context.ParameterInfo.ParameterType.Name}", $"Change the parameter type from {context.ParameterInfo.ParameterType.Name} to {typeof(T).Name}");
        }
    }
}

public abstract class FromAuthModelBinderString : FromAuthModelBinder<string>
{
    public override string GetDefault() => string.Empty;
}

public class AuthUserId : FromAuthModelBinder<Guid>
{
    public override Guid GetDefault() => Guid.Empty;
}

public class AuthSessionId : FromAuthModelBinderString;

public class AuthApiKey : FromAuthModelBinderString;

public class AuthPermission : FromAuthModelBinderString;

public class AuthPermissionService : FromAuthModelBinderString;

public class AuthType : FromAuthModelBinderString;

public class AuthUsername : FromAuthModelBinderString;
