using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace CredentialsAccessManager.Credentials.IdGenerators;

public class IdGenerator : IIdGenerator
{
    private IdGeneratorConfiguration Configuration;
    private static RandomNumberGenerator Random = RandomNumberGenerator.Create();
    private const int GuidByteLength = 16;

    public IdGenerator(IdGeneratorConfiguration configuration)
    {
        Configuration = configuration;
    }

    public (string userCompatibleId, byte[] databaseCompatibleId) GenerateId(Guid userId)
    {
        byte[] rawId = new byte[Configuration.IdLengthBytes];
        Random.GetBytes(rawId);
        string userCopy = Convert.ToBase64String(Combine(userId.ToByteArray(), rawId));
        return (userCopy, Configuration.Hasher(rawId));
    }

    public bool TryParseId(string userCompatibleId, [NotNullWhen(true)] out (Guid userId, byte[] databaseCompatibleId)? parsedId)
    {
        try
        {
            byte[] un64d = Convert.FromBase64String(userCompatibleId);
            if (un64d.Length <= 16)
            {
                parsedId = null;
                return false;
            }

            Guid userId = new Guid(un64d[..GuidByteLength]);
            byte[] rawSessionId = un64d[GuidByteLength..];
            parsedId = (userId, Configuration.Hasher(rawSessionId));
            return true;
        }
        catch
        {
            parsedId = null;
            return false;
        }
    }

    private static byte[] Combine(byte[] first, byte[] second)
    {
        byte[] bytes = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
        Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
        return bytes;
    }
}
