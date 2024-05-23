using AuthProvider.CamInterface;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace AuthProvider.Authentication;

public interface ICamAuthorizer
{
    public Task<AuthorizationResult> AuthenticateAsync(HttpRequest request, ICamInterface cam);

    public Task<AuthorizationResult> AuthorizeAsync(HttpRequest request, ICamInterface cam, string permission);

    public void ApplySwaggerGeneration(OpenApiOperation operation);
}
