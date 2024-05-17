using CredentialsAccessManager.Models;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace CredentialsAccessManager.User;

public class UserStore : IUserStore
{
    /// <summary>
    /// Dictionary of all Users - keyed by user id
    /// </summary>
    public ConcurrentDictionary<Guid, UserData> UserData = [];

    /// <summary>
    /// Mapping of usernames to user ids
    /// </summary>
    public ConcurrentDictionary<string, Guid> UserIds = [];

    public bool AttemptLogin(string username, string password, [NotNullWhen(true)] out Guid userId)
    {
        if (UserIds.TryGetValue(username, out userId))
        {
            if (UserData.TryGetValue(userId, out UserData? userData))
            {
                return PasswordHasher.Verify(password, userData.Password);
            }
        }

        return false;
    }

    public bool CreateUser(string username, string password, UserInformation userInformation)
    {
        if (UserIds.ContainsKey(username)) return false;
        var newUserId = Guid.NewGuid();
        if (!UserIds.TryAdd(username, newUserId)) return false;
        var newUserData = new UserData(username, PasswordHasher.Hash(password), userInformation);
        _ = UserData.TryAdd(newUserId, newUserData);
        return true;
    }

    public Guid? GetUserIdFromUsername(string username) => UserIds.TryGetValue(username, out Guid userId) ? userId : null;

    public string? GetUsernameFromUserId(Guid userId) => UserData.TryGetValue(userId, out UserData? userData) ? userData.Username : null;

    public bool HasPermission(Guid userId, string service, string permission)
    {
        if (UserData.TryGetValue(userId, out UserData? userData))
        {
            if (userData.Permissions == null) return false;

            if (userData.Permissions.TryGetValue(service, out HashSet<string>? permissions))
            {
                return permissions.Contains(permission);
            }
        }

        return false;
    }

    /// <summary>
    /// https://stackoverflow.com/questions/4181198/how-to-hash-a-password
    /// This was a very nice, compatible, secure solution
    /// </summary>
    public static class PasswordHasher
    {
        private const int _saltSize = 16; // 128 bits
        private const int _keySize = 32; // 256 bits
        private const int _iterations = 25000;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA512;

        private const char segmentDelimiter = ':';

        public static string Hash(string input)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(_saltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                _iterations,
                _algorithm,
                _keySize
            );
            return string.Join(
                segmentDelimiter,
                Convert.ToHexString(hash),
                Convert.ToHexString(salt),
                _iterations,
                _algorithm
            );
        }

        public static bool Verify(string input, string hashString)
        {
            string[] segments = hashString.Split(segmentDelimiter);
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
}
