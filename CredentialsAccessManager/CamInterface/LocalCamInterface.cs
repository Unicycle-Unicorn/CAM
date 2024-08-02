using AuthProvider.CamInterface;
using CredentialsAccessManager.Credentials.CredentialStore;

namespace CredentialsAccessManager.CamInterface;
internal class LocalCamInterface(string service, ICredentialStore credentialStore) : ICamInterface
{
    public string ServiceName { get; private set; } = service;
    private readonly ICredentialStore CredentialStore = credentialStore;

    public async Task Initialize()
    {
        foreach (var item in ICamInterface.Permissions)
        {
            Console.WriteLine(item);
        }
    }

    public async Task<AuthorizationResult> AuthenticateApiKeyAsync(string apiKey)
    {
        return CredentialStore.AuthenticateApiKey(apiKey);
    }

    public async Task<AuthorizationResult> AuthenticateCredentialsAsync(string username, string password)
    {
        return CredentialStore.AuthenticateCredentials(username, password);
    }
    public async Task<AuthorizationResult> AuthenticateSessionAsync(string sessionId)
    {
        return CredentialStore.AuthenticateSession(sessionId);
    }
    public async Task<AuthorizationResult> AuthenticateStrictSessionAsync(string sessionId, string password)
    {
        return CredentialStore.AuthenticateStrictSession(sessionId, password);
    }
    public async Task<AuthorizationResult> AuthorizeApiKeyAsync(string apiKey, string permission)
    {
        return CredentialStore.AuthorizeApiKey(apiKey, (ServiceName, permission));
    }
    public async Task<AuthorizationResult> AuthorizeCredentialsAsync(string username, string password, string permission)
    {
        return CredentialStore.AuthorizeCredentials(username, password, (ServiceName, permission));
    }
    public async Task<AuthorizationResult> AuthorizeSessionAsync(string sessionId, string permission)
    {
        return CredentialStore.AuthorizeSession(sessionId, (ServiceName, permission));
    }
    public async Task<AuthorizationResult> AuthorizeStrictSessionAsync(string sessionId, string password, string permission)
    {
        return CredentialStore.AuthorizeStrictSession(sessionId, password, (ServiceName, permission));
    }
    public async Task<UserActionResult<Guid>> GetUserIdFromUsernameAsync(string username)
    {
        return CredentialStore.GetUserIdFromUsername(username);
    }
    public async Task<UserActionResult<string>> GetUsernameFromUserIdAsync(Guid userId)
    {
        return CredentialStore.GetUsernameFromUserId(userId);
    }
}
