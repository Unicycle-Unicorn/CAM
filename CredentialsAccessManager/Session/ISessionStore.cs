namespace CredentialsAccessManager.Session;

public interface ISessionStore
{
    /// <summary>
    /// Byte length of Session id. This should be a minimum of 16
    /// </summary>
    const int SESSION_ID_LENGTH_BYTES = 16;

    /// <summary>
    /// Length of time (seconds) after no actions have been preformed till a Session is revoked
    /// </summary>
    const int SESSION_IDLE_TIMEOUT_SECONDS = 20;//20 * 60; // 20 minutes

    /// <summary>
    /// Length of time (seconds) till a Session is revoked. Useage does not matter
    /// </summary>
    const int SESSION_ABSOLUTE_TIMEOUT_SECONDS = 24 * 60 * 60; // 1 day

    /// <summary>
    /// Length of time (seconds) between Session garbage collector runs
    /// A good time is normally around half of SESSION_IDLE_TIMEOUT_SECONDS
    /// </summary>
    const int SESSION_GARBAGE_COLLECTOR_TIMER_SECONDS = 10 * 60; // 10 minutes

    /// <summary>
    /// Clock skew of authorizing sessions (this shouldn't be nescessary unless Session timeout is small)
    /// </summary>
    const int SESSION_CLOCK_SKEW_SECONDS = 0;

    public AuthProvider.SessionCredentials CreateNewSession(Guid userId);

    public bool AuthenticateSession(Guid userId, string sessionId);

    public void RevokeSession(Guid userId, string sessionId);

    public void RevokeAllSessions(Guid userId);
}
