namespace AuthProvider.CamInterface;

public class AuthorizationRequest
{
    public AuthRequestType AuthRequestType { get; set; }

    public string? ApiKey { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? SessionId { get; set; }

    private AuthorizationRequest(AuthRequestType type) => AuthRequestType = type;
    public AuthorizationRequest() { }

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
