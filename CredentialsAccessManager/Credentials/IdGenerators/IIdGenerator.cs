using System.Diagnostics.CodeAnalysis;

namespace CredentialsAccessManager.Credentials.IdGenerators;

public interface IIdGenerator
{
    /// <summary>
    /// Generates a new id compatible with the request headers, body, and cookies
    /// </summary>
    /// <param name="userId">UserId that the new id belongs to</param>
    /// <returns>A user compatible id for the user and a database compatible id for the database (normally hashed)</returns>
    public (string userCompatibleId, byte[] databaseCompatibleId) GenerateId(Guid userId);

    /// <summary>
    /// Attempts to parse the user compatible id into it's individual pieces
    /// </summary>
    /// <param name="userCompatibleId">The user compatible id to deconstruct</param>
    /// <param name="parsedId">The parsed id containing guid and the database compatible id</param>
    /// <returns>True if parsing attempt was successful</returns>
    public bool TryParseId(string userCompatibleId, [NotNullWhen(true)] out (Guid userId, byte[] databaseCompatibleId)? parsedId);
}
