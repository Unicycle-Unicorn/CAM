using System.Security.Cryptography;

namespace CredentialsAccessManager.Credentials.PasswordHashing;

public class PasswordHasherConfiguration
{
    public int SaltSize = 16; // 128 bits
    public int KeySize = 32; // 256 bits
    public int Interations = 25000;
    public readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;
    public char Delimitor = ':';
}
