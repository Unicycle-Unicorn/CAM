using System.Security.Cryptography;

namespace CredentialsAccessManager.Credentials.PasswordHashing;

/// <summary>
/// https://stackoverflow.com/questions/4181198/how-to-hash-a-password
/// This was a very nice, compatible, secure solution
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private PasswordHasherConfiguration Configuration;

    public PasswordHasher(PasswordHasherConfiguration configuration)
    {
        Configuration = configuration;
    }

    public string Hash(string input)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(Configuration.SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            Configuration.Interations,
            Configuration.Algorithm,
            Configuration.KeySize
        );

        return string.Join(
            Configuration.Delimitor,
            Convert.ToHexString(hash),
            Convert.ToHexString(salt),
            Configuration.Interations,
            Configuration.Algorithm
        );
    }

    public bool Verify(string input, string hashString)
    {
        string[] segments = hashString.Split(Configuration.Delimitor);
        byte[] hash = Convert.FromHexString(segments[0]);
        byte[] salt = Convert.FromHexString(segments[1]);
        int iterations = int.Parse(segments[2]);
        var algorithm = new HashAlgorithmName(segments[3]);
        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            iterations,
            algorithm,
            hash.Length
        );
        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}
