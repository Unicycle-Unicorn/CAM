using CredentialsAccessManager.Session;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;

namespace CredentialsAccessManager;

public class SessionStore : ISessionStore
{
    public RandomNumberGenerator Random = RandomNumberGenerator.Create();

    /// <summary>
    /// Dictionary of all (active sessions - keyed by Session id) - keyed by user id
    /// </summary>
    public ConcurrentDictionary<Guid, Dictionary<string, ActiveSession>> Sessions = [];

    /// <summary>
    /// Authenticates the current Session - refreshes if valid
    /// </summary>
    /// <param name="iPAddress">Ip address of the request origin</param>
    /// <param name="userId">User id of the requesting user</param>
    /// <param name="sessionId">Session id from the requesting user</param>
    /// <returns>Bool representing if this Session is valid</returns>
    public bool AuthenticateSession(Guid userId, string sessionId)
    {
        if (Sessions.TryGetValue(userId, out var userActiveSessions)) {
            if (userActiveSessions.TryGetValue(sessionId, out var activeSession))
            {
                long currentTime = Utils.GetUnixTime();

                // Remove Session if expired and return false
                if (activeSession.HasExpired(currentTime + ISessionStore.SESSION_CLOCK_SKEW_SECONDS))
                {
                    _ = userActiveSessions.Remove(sessionId);
                    return false;
                }

                // Session is valid - refresh it and return
                activeSession.Refresh(currentTime + ISessionStore.SESSION_IDLE_TIMEOUT_SECONDS);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a new Session
    /// </summary>
    /// <param name="iPAddress">Ip address of the Session originator</param>
    /// <param name="userId">User id of the Session owner</param>
    /// <returns>The new Session</returns>
    public AuthProvider.SessionCredentials CreateNewSession(Guid userId)
    {
        byte[] rawSessionId = new byte[ISessionStore.SESSION_ID_LENGTH_BYTES];
        Random.GetBytes(rawSessionId);
        string sessionId = Convert.ToBase64String(rawSessionId);

        long currentTime = Utils.GetUnixTime();
        var newSession = new ActiveSession(currentTime, currentTime + ISessionStore.SESSION_IDLE_TIMEOUT_SECONDS, currentTime + ISessionStore.SESSION_ABSOLUTE_TIMEOUT_SECONDS);

        if (Sessions.TryGetValue(userId, out var userSessions))
        {
            // User already exists in map
            userSessions.Add(sessionId, newSession);
        } else
        {
            // User not yet present in map
            _ = Sessions.TryAdd(userId, new() {{sessionId, newSession}});
        }

        return new AuthProvider.SessionCredentials() { UserId = userId, SessionId = sessionId };
    }

    /// <summary>
    /// Revokes all sessions belonging to a specific user <b>Global Sign Out</b>
    /// </summary>
    /// <param name="userId">User id of the requesting user</param>
    public void RevokeAllSessions(Guid userId)
    {
        _ = Sessions.Remove(userId, out _);
    }

    /// <summary>
    /// Revokes specific sessions belonging to a specific user. <b>Session Sign Out</b>
    /// </summary>
    /// <param name="userId">User id of the requesting user</param>
    /// <param name="sessionId">Session id of the Session to be revoked</param>
    public void RevokeSession(Guid userId, string sessionId)
    {
        if (Sessions.TryGetValue(userId, out var activeSessions))
        {
            activeSessions.Remove(sessionId);
        }
    }
}
