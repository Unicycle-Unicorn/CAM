using Microsoft.OpenApi.Models;

namespace AuthProvider.Authentication;

internal static class OpenApiOperationExtensions
{
    internal static void AddOptionalRequestCookie(this OpenApiOperation operation, string name)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Cookie,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }

    internal static void AddOptionalRequestHeader(this OpenApiOperation operation, string name)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }

    internal static void AddResponse(this OpenApiOperation operation, string response, string description)
    {
        operation.Responses.Add(response, new OpenApiResponse
        {
            Description = description
        });
    }
}
