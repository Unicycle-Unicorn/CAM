namespace CredentialsAccessManager.Utils;

public static class TimeUtils
{
    public static long GetUnixTime() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}