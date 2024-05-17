namespace CredentialsAccessManager;

public static class Utils
{
    public static long GetUnixTime() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}