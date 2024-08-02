namespace CredentialsAccessManager.Credentials;


public class ApiKey(long currentTime, string id, Permissions permissions)
{
    /// <summary>
    /// Creation time of this api key
    /// </summary>
    public long CreationTime { get; set; } = currentTime;

    /// <summary>
    /// Last time this api key was used
    /// </summary>
    public long LastUseTime { get; set; }

    /// <summary>
    /// Permissions of this api key
    /// </summary>
    public Permissions Permissions { get; set; } = permissions;

    /// <summary>
    /// Internal identifier of this ApiKey (likely the last few bytes of the secure id)
    /// </summary>
    public string InternalId { get; } = id;

    public void Use(long lastUseTime) => LastUseTime = lastUseTime;
}
