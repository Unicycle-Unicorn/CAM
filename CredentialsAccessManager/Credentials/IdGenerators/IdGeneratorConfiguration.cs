namespace CredentialsAccessManager.Credentials.IdGenerators;

public class IdGeneratorConfiguration
{
    /// <summary>
    /// Byte length of the id. This should be a minimum of 16
    /// </summary>
    public int IdLengthBytes = 16;

    /// <summary>
    /// The function which generates an id hash. This defaults to none
    /// </summary>
    public Func<byte[], byte[]> Hasher = (byte[] bytes) => bytes;
}
