using CredentialsAccessManager.Credentials.IdGenerators;
using CredentialsAccessManager.Credentials.PasswordHashing;

namespace CredentialsAccessManager.Credentials.CredentialStore;

public class CredentialStoreConfiguration
{
    /// <summary>
    /// Generator with the functionality to generate new api key ids
    /// </summary>
    public IIdGenerator ApiKeyIdGenerator = new IdGenerator(new());

    /// <summary>
    /// Generator with the functionality to generate new session ids
    /// </summary>
    public IIdGenerator SessionIdGenerator = new IdGenerator(new());

    /// <summary>
    /// Password hasher
    /// </summary>
    public IPasswordHasher PasswordHasher = new PasswordHasher(new());

    /// <summary>
    /// Length of time (seconds) after no actions have been preformed till a Session is revoked
    /// </summary>
    public int SessionIdleTimeoutSeconds = 20 * 60; // 20 minutes

    /// <summary>
    /// Length of time (seconds) till a Session is revoked. Useage does not matter
    /// </summary>
    public int SessionAbsoluteTimeoutSeconds = 24 * 60 * 60; // 1 day

    /// <summary>
    /// Length of time (seconds) between Session garbage collector runs
    /// A good time is normally around half of SESSION_IDLE_TIMEOUT_SECONDS
    /// </summary>
    public int SessionGarbageCollectionSeconds = 10 * 60; // 10 minutes

    /// <summary>
    /// Clock skew of authorizing sessions (this shouldn't be nescessary unless Session timeout is small)
    /// </summary>
    public int SessionClockSkewSeconds = 0;

    /// <summary>
    /// Default permissions of all new users
    /// </summary>
    public Permissions DefaultUserPermissions = new();
}
