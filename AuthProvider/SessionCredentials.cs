namespace AuthProvider;

public class SessionCredentials
{
    public Guid UserId { get; set; }
    public required string SessionId { get; set; }

    public override string ToString() => $"{UserId}:{SessionId}";

    public static SessionCredentials? FromString(string session)
    {
        string[] segments = session.Split(':');
        if (segments.Length == 2)
        {
            try
            {
                var userId = Guid.Parse(segments[0]);
                string sessionId = segments[1];
                return new SessionCredentials() { UserId = userId, SessionId = sessionId };
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}
