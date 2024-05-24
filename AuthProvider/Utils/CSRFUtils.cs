using System.Security.Cryptography;

namespace AuthProvider.Utils;

public static class CSRFUtils
{
    public static bool TryGenerateCSRF(string sessionId, out string csrf)
    {
        try
        {
            byte[] rawSessionId = Convert.FromBase64String(sessionId);
            byte[] rawCSRF = SHA256.HashData(rawSessionId);
            csrf = Convert.ToBase64String(rawCSRF);
            return true;
        } catch
        {
            csrf = "";
            return false;
        }
        
    }

    public static bool VerifyCSRF(string sessionId, string csrf) => TryGenerateCSRF(sessionId, out string expected) && csrf == expected;
}
