using System.Net.Http.Json;

namespace AuthProvider.CamInterface;
public class RemoteCamInterface(string service, string url) : ICamInterface
{
    private readonly HttpClient CamClient = new();
    private readonly string Remote = url;

    public async Task Initialize()
    {
        foreach (var item in ICamInterface.Permissions)
        {
            Console.WriteLine(item);
        }
    }

    public string ServiceName { get; private set; } = service;

    public async Task<AuthorizationResult> AuthenticateApiKeyAsync(string apiKey) => throw new NotImplementedException();
    public async Task<AuthorizationResult> AuthenticateCredentialsAsync(string username, string password) => throw new NotImplementedException();
    public async Task<AuthorizationResult> AuthenticateSessionAsync(string sessionId) => throw new NotImplementedException();
    public async Task<AuthorizationResult> AuthenticateStrictSessionAsync(string sessionId, string password) => throw new NotImplementedException();
    public async Task<AuthorizationResult> AuthorizeApiKeyAsync(string apiKey, string permission) => throw new NotImplementedException();
    public async Task<AuthorizationResult> AuthorizeCredentialsAsync(string username, string password, string permission) => throw new NotImplementedException();
    public async Task<AuthorizationResult> AuthorizeSessionAsync(string sessionId, string permission) => throw new NotImplementedException();
    public async Task<AuthorizationResult> AuthorizeStrictSessionAsync(string sessionId, string password, string permission) => throw new NotImplementedException();
    public async Task<UserActionResult<Guid>> GetUserIdFromUsernameAsync(string username) => throw new NotImplementedException();
    public async Task<UserActionResult<string>> GetUsernameFromUserIdAsync(Guid userId) => throw new NotImplementedException();
}
