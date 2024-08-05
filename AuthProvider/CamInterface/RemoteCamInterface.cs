using System.Net.Http.Json;

namespace AuthProvider.CamInterface;

public class RemoteCamInterface(string service, string url) : ICamInterface
{
    private readonly HttpClient CamClient = new();
    private readonly string Remote = url;
    public string ServiceName { get; private set; } = service;

    public async Task Initialize()
    {
        foreach (var item in ICamInterface.Permissions)
        {
            Console.WriteLine(item);
        }
    }

    public async Task<AuthorizationResult> AuthenticateApiKeyAsync(string apiKey) => await PostAuthenticationRequest(AuthorizationRequest.WithApiKey(apiKey));
    public async Task<AuthorizationResult> AuthenticateCredentialsAsync(string username, string password) => await PostAuthenticationRequest(AuthorizationRequest.WithCredentials(username, password));
    public async Task<AuthorizationResult> AuthenticateSessionAsync(string sessionId) => await PostAuthenticationRequest(AuthorizationRequest.WithSession(sessionId));
    public async Task<AuthorizationResult> AuthenticateStrictSessionAsync(string sessionId, string password) => await PostAuthenticationRequest(AuthorizationRequest.WithStricSession(sessionId, password));
    public async Task<AuthorizationResult> AuthorizeApiKeyAsync(string apiKey, string permission) => await PostAuthorizationRequest(AuthorizationRequest.WithApiKey(apiKey), permission);
    public async Task<AuthorizationResult> AuthorizeCredentialsAsync(string username, string password, string permission) => await PostAuthorizationRequest(AuthorizationRequest.WithCredentials(username, password), permission);
    public async Task<AuthorizationResult> AuthorizeSessionAsync(string sessionId, string permission) => await PostAuthorizationRequest(AuthorizationRequest.WithSession(sessionId), permission);
    public async Task<AuthorizationResult> AuthorizeStrictSessionAsync(string sessionId, string password, string permission) => await PostAuthorizationRequest(AuthorizationRequest.WithStricSession(sessionId, password), permission);

    public async Task<UserActionResult<Guid>> GetUserIdFromUsernameAsync(string username) => throw new NotImplementedException();
    public async Task<UserActionResult<string>> GetUsernameFromUserIdAsync(Guid userId) => throw new NotImplementedException();

    private async Task<AuthorizationResult> PostAuthenticationRequest(AuthorizationRequest authRequest)
    {
        return await PostRequest<AuthorizationResult, AuthorizationRequest>($"{Remote}/internal/authenticate/{ServiceName}", authRequest);
    }

    private async Task<AuthorizationResult> PostAuthorizationRequest(AuthorizationRequest authRequest, string permission)
    {
        return await PostRequest<AuthorizationResult, AuthorizationRequest>($"{Remote}/internal/authorize/{ServiceName}/{permission}", authRequest);
    }

    private async Task<Resp> PostRequest<Resp, Req>(string Url, Req req)
    {
        var response = await CamClient.PostAsJsonAsync(Url, req);
        if (response.IsSuccessStatusCode)
        {
            Resp authResult = await response.Content.ReadFromJsonAsync<Resp>();
            return authResult;
        } else
        {
            throw new NotImplementedException();
        }
    }
}