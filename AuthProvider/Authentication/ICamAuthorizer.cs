using AuthProvider.CamInterface;
using Microsoft.AspNetCore.Http;

namespace AuthProvider.Authentication;

public interface ICamAuthorizer
{
    public Task<AuthorizationResult> AuthenticateAsync(HttpRequest request, IServiceProvider services);

    public Task<AuthorizationResult> AuthorizeAsync(HttpRequest request, IServiceProvider services, string permission);
}
