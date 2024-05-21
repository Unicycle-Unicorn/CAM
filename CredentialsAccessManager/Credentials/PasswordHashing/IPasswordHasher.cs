namespace CredentialsAccessManager.Credentials.PasswordHashing;

public interface IPasswordHasher
{
    public string Hash(string password);

    public bool Verify(string input, string hashString);
}
