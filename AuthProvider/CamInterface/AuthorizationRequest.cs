namespace AuthProvider.CamInterface;

public class AuthorizationRequest
{
    public AuthRequestType AuthRequestType;

    public string? ApiKey;
    public string? Username;
    public string? Password;
    public string? SessionId;

    private AuthorizationRequest(AuthRequestType type) => AuthRequestType = type;

    public static AuthorizationRequest WithApiKey(string apiKey) => new(AuthRequestType.ApiKey) { ApiKey = apiKey };
    public static AuthorizationRequest WithCredentials(string username, string password) => new(AuthRequestType.Credentials) { Username = username, Password = password };
    public static AuthorizationRequest WithSession(string sessionId) => new(AuthRequestType.Session) { SessionId = sessionId};
    public static AuthorizationRequest WithStricSession(string sessionId, string password) => new(AuthRequestType.StrictSession) { SessionId = sessionId, Password = password };
}

public enum AuthRequestType : byte
{
    ApiKey,
    Credentials,
    Session,
    StrictSession
}
