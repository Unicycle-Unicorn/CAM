using CredentialsAccessManager.Session;
using System.Diagnostics.CodeAnalysis;

using UserId = System.Guid;
using Username = string;
using Password = string;
using HashedPassword = string;
using ApiKeyId = string;
using HashedApiKeyId = byte[];
using SessionId = string;
using HashedSessionId = byte[];
using SinglePermission = (string service, string permission);
using CredentialsAccessManager.Models;
using System.Collections.Concurrent;

namespace CredentialsAccessManager.Credentials;

public class CredentialStore : ICredentialStore
{
    private ConcurrentDictionary<UserId, UserData> UserIdsToUserData = [];
    private ConcurrentDictionary<Username, UserId> UsernamesToUserIds = new(StringComparer.OrdinalIgnoreCase);
    private CredentialStoreConfiguration Configuration;

    public CredentialStore(CredentialStoreConfiguration configuration)
    {
        Configuration = configuration;
    }

    #region User
    public bool TryCreateUser(Username username, Password password)
    {
        if (UsernamesToUserIds.ContainsKey(username)) return false;
        UserId newUserId = UserId.NewGuid();
        if (!UsernamesToUserIds.TryAdd(username, newUserId)) return false;
        UserData newUserData = new UserData()
        {
            Username = username,
            HashedPassword = Configuration.PasswordHasher.Hash(password),
            Permissions = Configuration.DefaultUserPermissions.Duplicate()
        };
        _ = UserIdsToUserData.TryAdd(newUserId, newUserData);
        return true;
    }
    public AuthorizationResult AttemptAuthenticateCredentials(Username username, Password password)
    {
        if (UsernamesToUserIds.TryGetValue(username, out UserId userId))
        {
            if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
            {
                if (Configuration.PasswordHasher.Verify(password, userData.HashedPassword)) {
                    return AuthorizationResult.Authenticated(userId);
                }
            }
        }
        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AttemptAuthorizeCredentials(Username username, Password password, SinglePermission permission)
    {
        if (UsernamesToUserIds.TryGetValue(username, out UserId userId))
        {
            if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
            {
                if (Configuration.PasswordHasher.Verify(password, userData.HashedPassword))
                {
                    if (userData.Permissions.Contains(permission))
                    {
                        return AuthorizationResult.Authorized(userId, permission);
                    }
                    return AuthorizationResult.Authenticated(userId);
                }
            }
        }
        return AuthorizationResult.Failed();
    }
    public bool TryGetUserIdFromUsername(Username username, [NotNullWhen(true)] out UserId userId)
    {
        return (UsernamesToUserIds.TryGetValue(username, out userId));
    }
    public bool TryGetUsernameFromUserId(UserId userId, [NotNullWhen(true)] out Username? username)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            username = userData.Username;
            return true;
        }

        username = null;
        return false;
    }
    // public bool TryGetUserPermissions(UserId userId, [NotNullWhen(true)] out Permissions? username);
    // public bool GrantUserPermission(UserId userId, Permission permission);
    // public bool RevokeUserPermission(UserId userId, Permission permission);
    // public bool TryGetUserInformation(UserId userId, [NotNullWhen(true)] out UserInformation userInformation);
    // public bool UpdateUserInformation(UserId userId, UserInformation newUserInformation);
    // public bool UpdateUsername(UserId userId, Username username);
    // public bool UpdatePassword(UserId userId, Password password);
    #endregion


    #region Session
    public bool TryCreateNewSession(UserId userId, [NotNullWhen(true)] out SessionId? sessionId)
    {
        (string userCompatibleId, byte[] databaseCompatibleId) = Configuration.SessionIdGenerator.GenerateId(userId);
        long currentTime = Utils.GetUnixTime();
        ActiveSession session = new ActiveSession(currentTime, currentTime + Configuration.SessionIdleTimeoutSeconds, currentTime + Configuration.SessionAbsoluteTimeoutSeconds);

        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.Sessions != null)
            {
                userData.Sessions.Add(databaseCompatibleId, session);
            } else
            {
                userData.Sessions = new Dictionary<HashedSessionId, ActiveSession>(StructuralEqualityComparer<HashedSessionId>.Default);
            }
            sessionId = userCompatibleId;
            return true;
        }

        sessionId = null;
        return false;
    }
    public AuthorizationResult AttemptAuthenticateSession(SessionId sessionId)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                if (userData.Sessions.TryGetValue(parsedId.Value.databaseCompatibleId, out ActiveSession? session) && session != null)
                {
                    long currentTime = Utils.GetUnixTime();

                    // Remove Session if expired and return false
                    if (!session.HasExpired(currentTime + Configuration.SessionClockSkewSeconds))
                    {
                        // Session is valid - refresh it and return
                        session.Refresh(currentTime + Configuration.SessionIdleTimeoutSeconds);
                        return AuthorizationResult.Authenticated(parsedId.Value.userId);
                    }
                    
                    _ = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AttemptAuthorizeSession(SessionId sessionId, SinglePermission permission)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                if (userData.Sessions.TryGetValue(parsedId.Value.databaseCompatibleId, out ActiveSession? session) && session != null)
                {
                    long currentTime = Utils.GetUnixTime();

                    // Remove Session if expired and return false
                    if (!session.HasExpired(currentTime + Configuration.SessionClockSkewSeconds))
                    {
                        // Session is valid - refresh it and return
                        session.Refresh(currentTime + Configuration.SessionIdleTimeoutSeconds);

                        if (userData.Permissions.Contains(permission))
                        {
                            return AuthorizationResult.Authorized(parsedId.Value.userId, permission);
                        } else
                        {
                            return AuthorizationResult.Authenticated(parsedId.Value.userId);
                        }
                    }

                    _ = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AttemptAuthenticateStrictSession(SessionId sessionId, Password password)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                if (userData.Sessions.TryGetValue(parsedId.Value.databaseCompatibleId, out ActiveSession? session) && session != null)
                {
                    long currentTime = Utils.GetUnixTime();

                    // Remove Session if expired and return false
                    if (!session.HasExpired(currentTime + Configuration.SessionClockSkewSeconds))
                    {
                        // Session is valid - refresh it and return
                        session.Refresh(currentTime + Configuration.SessionIdleTimeoutSeconds);

                        if (Configuration.PasswordHasher.Verify(password, userData.HashedPassword))
                        {
                            return AuthorizationResult.Authenticated(parsedId.Value.userId);
                        }
                        
                    }

                    _ = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AttemptAuthorizeStrictSession(SessionId sessionId, Password password, SinglePermission permission)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                if (userData.Sessions.TryGetValue(parsedId.Value.databaseCompatibleId, out ActiveSession? session) && session != null)
                {
                    long currentTime = Utils.GetUnixTime();

                    // Remove Session if expired and return false
                    if (!session.HasExpired(currentTime + Configuration.SessionClockSkewSeconds))
                    {
                        // Session is valid - refresh it and return
                        session.Refresh(currentTime + Configuration.SessionIdleTimeoutSeconds);

                        if (Configuration.PasswordHasher.Verify(password, userData.HashedPassword))
                        {
                            if (userData.Permissions.Contains(permission))
                            {
                                return AuthorizationResult.Authorized(parsedId.Value.userId, permission);
                            } else
                            {
                                return AuthorizationResult.Authenticated(parsedId.Value.userId);
                            }
                        }

                    }

                    _ = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public bool RevokeSessionBySessionId(SessionId sessionId)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                bool removed = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                if (userData.Sessions.Count <= 0)
                {
                    userData.Sessions = null;
                }
                return removed;
            }
        }
        return false;
    }
    public bool RevokeSessionBySessionInternalId(UserId userId, int internalSessionId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData?.Sessions != null)
        {
            byte[]? sessionIdToRemove = null;
            foreach ((byte[] sessionId, ActiveSession session) in userData.Sessions)
            {
                if (session.InternalSessionId == internalSessionId)
                {
                    sessionIdToRemove = sessionId;
                    break;
                }
            }

            if (sessionIdToRemove != null)
            {
                bool removed = userData.Sessions.Remove(sessionIdToRemove);
                if (userData.Sessions.Count <= 0)
                {
                    userData.Sessions = null;
                }
                return removed;
            }
        }

        return false;
    }
    public bool RevokeAllSessions(UserId userId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData?.Sessions != null)
        {
            userData.Sessions.Clear();
            userData.Sessions = null;
            return true;
        }

        return false;
    }
    public bool TryGetAllSessions(UserId userId, [NotNullWhen(true)] out List<ActiveSession>? sessions)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData?.Sessions != null)
        {
            sessions = [.. userData.Sessions.Values];
            return true;
        }

        sessions = null;
        return false;
    }
    #endregion


    #region ApiKey
    public bool TryCreateNewApiKey(UserId userId, Permissions permissions, [NotNullWhen(true)] out ApiKeyId? apiKeyid)
    {
        (string userCompatibleId, byte[] databaseCompatibleId) = Configuration.ApiKeyIdGenerator.GenerateId(userId);

        // Take the last 4 significant characters of the user compatible id
        string trimmed = userCompatibleId.TrimEnd('=');
        string internalKeyId = $"{trimmed[^4..]}{new String('=', userCompatibleId.Length - trimmed.Length)}";

        long currentTime = Utils.GetUnixTime();
        ApiKey apiKey = new ApiKey(currentTime, internalKeyId, permissions);
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.ApiKeys != null)
            {
                userData.ApiKeys.Add(databaseCompatibleId, apiKey);
            }
            else
            {
                userData.ApiKeys = new Dictionary<HashedApiKeyId, ApiKey>(StructuralEqualityComparer<HashedApiKeyId>.Default);
            }
            apiKeyid = userCompatibleId;
            return true;
        }

        apiKeyid = null;
        return false;
    }
    public AuthorizationResult AttemptAuthenticateApiKey(ApiKeyId apiKeyId)
    {
        if (Configuration.ApiKeyIdGenerator.TryParseId(apiKeyId, out (UserId userId, HashedApiKeyId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.ApiKeys != null)
            {
                if (userData.ApiKeys.TryGetValue(parsedId.Value.databaseCompatibleId, out ApiKey? apiKey) && apiKey != null)
                {
                    long currentTime = Utils.GetUnixTime();

                    apiKey.Use(currentTime);

                    return AuthorizationResult.Authenticated(parsedId.Value.userId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AttemptAuthorizeApiKey(ApiKeyId apiKeyId, SinglePermission permission)
    {
        if (Configuration.ApiKeyIdGenerator.TryParseId(apiKeyId, out (UserId userId, HashedApiKeyId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.ApiKeys != null)
            {
                if (userData.ApiKeys.TryGetValue(parsedId.Value.databaseCompatibleId, out ApiKey? apiKey) && apiKey != null)
                {
                    long currentTime = Utils.GetUnixTime();

                    apiKey.Use(currentTime);

                    if (apiKey.Permissions.Contains(permission) && userData.Permissions.Contains(permission))
                    {
                        return AuthorizationResult.Authorized(parsedId.Value.userId, permission);
                    } else
                    {
                        return AuthorizationResult.Authenticated(parsedId.Value.userId);
                    }
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public bool RevokeApiKeyPermissionByApiKeyInternalId(UserId userId, string internalId, SinglePermission permission)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData?.ApiKeys != null)
        {
            foreach ((byte[] apiKeyId, ApiKey apiKey) in userData.ApiKeys)
            {
                if (apiKey.InternalId == internalId)
                {
                    return apiKey.Permissions.Remove(permission);
                }
            }
        }

        return false;
    }
    public bool DeleteApiKeyByApiKeyInternalId(UserId userId, string internalId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData?.ApiKeys != null)
        {
            byte[]? apiKeyidToRemove = null;
            foreach ((byte[] apiKeyId, ApiKey apiKey) in userData.ApiKeys)
            {
                if (apiKey.InternalId == internalId)
                {
                    apiKeyidToRemove = apiKeyId;
                    break;
                }
            }

            if (apiKeyidToRemove != null)
            {
                bool removed = userData.ApiKeys.Remove(apiKeyidToRemove);
                if (userData.ApiKeys.Count <= 0)
                {
                    userData.ApiKeys = null;
                }
                return removed;
            }
        }

        return false;
    }
    public bool TryGetAllApiKeys(UserId userId, [NotNullWhen(true)] out List<ApiKey>? apiKeys)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData?.ApiKeys != null)
        {
            apiKeys = [.. userData.ApiKeys.Values];
            return true;
        }

        apiKeys = null;
        return false;
    }
    #endregion
}

internal class UserData
{
    public Username Username { get; set; }
    public HashedPassword HashedPassword { get; set; }
    public UserInformation? UserInformation { get; set; } = null;
    public Permissions Permissions { get; set; }
    public Dictionary<HashedSessionId, ActiveSession>? Sessions { get; set; } = null;
    public Dictionary<HashedApiKeyId, ApiKey>? ApiKeys { get; set; } = null;
}