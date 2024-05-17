namespace CredentialsAccessManager.Session;

public class ActiveSession
{
    /// <summary>
    /// Creation time of this Session
    /// </summary>
    public long CreationTime { get; set; }

    /// <summary>
    /// Idle expiry time of this Session
    /// </summary>
    public long IdleExpiryTime { get; set; }

    /// <summary>
    /// Absolute expiry time of this Session
    /// </summary>
    public long AbsoluteExpiryTime { get; set; }

    public ActiveSession(long currentTime, long idleExpiryTime, long absoluteExpiryTime)
    {
        CreationTime = currentTime;
        IdleExpiryTime = idleExpiryTime;
        AbsoluteExpiryTime = absoluteExpiryTime;
    }

    /// <summary>
    /// Determines if this Session has expired
    /// </summary>
    /// <param name="comparedTime">Time including clock skew</param>
    /// <returns>Bool representing if this Session has expired</returns>
    public bool HasExpired(long comparedTime) => IdleExpiryTime < comparedTime || AbsoluteExpiryTime < comparedTime;

    public void Refresh(long idleExpiryTime) => IdleExpiryTime = idleExpiryTime;
}
