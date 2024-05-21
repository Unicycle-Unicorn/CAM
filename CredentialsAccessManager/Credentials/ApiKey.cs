namespace CredentialsAccessManager.Credentials;


public class ApiKey
{
    /// <summary>
    /// Creation time of this api key
    /// </summary>
    public long CreationTime { get; set; }

    /// <summary>
    /// Last time this api key was used
    /// </summary>
    public long LastUseTime { get; set; }

    /// <summary>
    /// Permissions of this api key
    /// </summary>
    public Permissions Permissions { get; set; }

    /// <summary>
    /// Internal identifier of this ApiKey (likely the last few bytes of the secure id)
    /// </summary>
    public string InternalId { get; }

    public ApiKey(long currentTime, string id, Permissions permissions)
    {
        CreationTime = currentTime;
        Permissions = permissions;
        InternalId = id;
    }

    public void Use(long lastUseTime) => LastUseTime = lastUseTime;
}
