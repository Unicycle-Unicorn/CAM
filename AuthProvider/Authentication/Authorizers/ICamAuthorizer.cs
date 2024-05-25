using AuthProvider.AuthModelBinder;
using AuthProvider.CamInterface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace AuthProvider.Authentication.Authorizers;

public interface ICamAuthorizer
{
    public Task<(AuthorizationResult, Action)> AuthenticateAsync(HttpContext context, ICamInterface cam);

    public Task<(AuthorizationResult, Action)> AuthorizeAsync(HttpContext context, ICamInterface cam, string permission);
    
    public abstract static Type[] ProvidedItemsDuringAuthorization();
    public abstract static Type[] ProvidedItemsDuringAuthentication();

    public void ApplySwaggerGeneration(OpenApiOperation operation);
}