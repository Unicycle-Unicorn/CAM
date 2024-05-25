using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace AuthProvider.Swagger;

public class SwaggerAuth : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attrs = context.MethodInfo.GetCustomAttributes<AuthAttribute>();
        if (attrs.Any())
        {
            var attr = attrs.First();
            attr.GenerateSwagger(operation, context);
        }
    }
}
