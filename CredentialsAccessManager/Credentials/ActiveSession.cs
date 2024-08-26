namespace CredentialsAccessManager.Credentials;

public class ActiveSession(long currentTime, long idleExpiryTime, long absoluteExpiryTime, string clientIpAddress)
{

    private static readonly Random random = new();
    public static int GetInsecureRandomId()
    {
        lock (random) // synchronize
        {
            return random.Next();
        }
    }

    /// <summary>
    ///	Client's initial ip address when session first created.
    ///	Should be used to prevent session duplication accross the same IP.
    /// </summary>
    public string InitialSessionIp { get; set; } = clientIpAddress;
    
    /// <summary>
    /// Creation time of this Session
    /// </summary>
    public long CreationTime { get; set; } = currentTime;

    /// <summary>
    /// Idle expiry time of this Session
    /// </summary>
    public long IdleExpiryTime { get; set; } = idleExpiryTime;

    /// <summary>
    /// Absolute expiry time of this Session
    /// </summary>
    public long AbsoluteExpiryTime { get; set; } = absoluteExpiryTime;

    /// <summary>
    /// Internal session identifier - this way hashes are not needed
    /// </summary>
    public int InternalSessionId { get; } = GetInsecureRandomId();

    /// <summary>
    /// Determines if this Session has expired
    /// </summary>
    /// <param name="comparedTime">Time including clock skew</param>
    /// <returns>Bool representing if this Session has expired</returns>
    public bool HasExpired(long comparedTime) => IdleExpiryTime < comparedTime || AbsoluteExpiryTime < comparedTime;

    public void Refresh(long idleExpiryTime) => IdleExpiryTime = idleExpiryTime;
}
