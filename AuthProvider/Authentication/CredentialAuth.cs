using AuthProvider.CamInterface;
using AuthProvider.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace AuthProvider.Authentication;

public class CredentialAuth : ICamAuthorizer
{
    public async Task<AuthorizationResult> AuthenticateAsync(HttpRequest request, ICamInterface cam)
    {
        Console.WriteLine("Inside CredentialAuth - Authenticate");
        if (HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthUser, out string XAuthUser) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthPass, out string XAuthPass))
        {
            return await cam.AuthenticateCredentialsAsync(XAuthUser, XAuthPass);
        }

        return AuthorizationResult.Failed();
    }
    public async Task<AuthorizationResult> AuthorizeAsync(HttpRequest request, ICamInterface cam, string permission)
    {
        Console.WriteLine("Inside CredentialAuth - Authorize");
        
        if (HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthUser, out string XAuthUser) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthPass, out string XAuthPass))
        {
            return await cam.AuthorizeCredentialsAsync(XAuthUser, XAuthPass, permission);
        }

        return AuthorizationResult.Failed();
    }

    public void ApplySwaggerGeneration(OpenApiOperation operation)
    {
        operation.AddOptionalResponseHeader(HeaderUtils.XAuthUser);
        operation.AddOptionalResponseHeader(HeaderUtils.XAuthPass);
    }
}
