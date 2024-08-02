using AuthProvider.CamInterface;
using CredentialsAccessManager.Utils;
using System.Collections.Concurrent;
using ApiKeyId = string;
using HashedApiKeyId = byte[];
using HashedPassword = string;
using HashedSessionId = byte[];
using Password = string;
using SessionId = string;
using SinglePermission = (string service, string permission);
using UserId = System.Guid;
using Username = string;

namespace CredentialsAccessManager.Credentials.CredentialStore;

public class CredentialStore(CredentialStoreConfiguration configuration) : ICredentialStore
{
    private readonly ConcurrentDictionary<UserId, UserData> UserIdsToUserData = [];
    private readonly ConcurrentDictionary<Username, UserId> UsernamesToUserIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly CredentialStoreConfiguration Configuration = configuration;

    #region User
    public UserActionResult<UserId> CreateUser(Username username, Password password)
    {
        if (UsernamesToUserIds.ContainsKey(username)) return UserActionResult<UserId>.Unsuccessful();
        UserId newUserId = UserId.NewGuid();
        if (!UsernamesToUserIds.TryAdd(username, newUserId)) return UserActionResult<UserId>.Unsuccessful();
        UserData newUserData = new UserData()
        {
            Username = username,
            HashedPassword = Configuration.PasswordHasher.Hash(password),
            Permissions = Configuration.DefaultUserPermissions.Duplicate()
        };
        _ = UserIdsToUserData.TryAdd(newUserId, newUserData);
        return UserActionResult<UserId>.Successful(newUserId);
    }
    public AuthorizationResult AuthenticateCredentials(Username username, Password password)
    {
        if (UsernamesToUserIds.TryGetValue(username, out UserId userId))
        {
            if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
            {
                if (Configuration.PasswordHasher.Verify(password, userData.HashedPassword))
                {
                    return AuthorizationResult.Authenticated(userId, userData.Username);
                }
            }
        }
        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AuthorizeCredentials(Username username, Password password, SinglePermission permission)
    {
        if (UsernamesToUserIds.TryGetValue(username, out UserId userId))
        {
            if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
            {
                if (Configuration.PasswordHasher.Verify(password, userData.HashedPassword))
                {
                    if (userData.Permissions.Contains(permission))
                    {
                        return AuthorizationResult.Authorized(userId, userData.Username, permission);
                    }
                    return AuthorizationResult.Authenticated(userId, userData.Username);
                }
            }
        }
        return AuthorizationResult.Failed();
    }
    public UserActionResult<UserId> GetUserIdFromUsername(Username username)
    {
        if (UsernamesToUserIds.TryGetValue(username, out UserId userId))
        {
            return UserActionResult<UserId>.Successful(userId);
        }

        return UserActionResult<UserId>.UserNotFound();
    }
    public UserActionResult<Username> GetUsernameFromUserId(UserId userId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            return UserActionResult<Username>.Successful(userData.Username);
        }

        return UserActionResult<Username>.UserNotFound();
    }
    // public UserActionResult<Permissions> GetUserPermissions(UserId userId);
    // public UserActionResult GrantUserPermission(UserId userId, Permission permission);
    // public UserActionResult RevokeUserPermission(UserId userId, Permission permission);
    // public UserActionResult<UserInformation> GetUserInformation(UserId userId);
    // public UserActionResult UpdateUserInformation(UserId userId, UserInformation newUserInformation);
    // public UserActionResult UpdateUsername(UserId userId, Username username);
    // public UserActionResult UpdatePassword(UserId userId, Password password);
    #endregion


    #region Session
    public UserActionResult<SessionId> CreateNewSession(UserId userId)
    {
        (string userCompatibleId, byte[] databaseCompatibleId) = Configuration.SessionIdGenerator.GenerateId(userId);
        long currentTime = TimeUtils.GetUnixTime();
        ActiveSession session = new ActiveSession(currentTime, currentTime + Configuration.SessionIdleTimeoutSeconds, currentTime + Configuration.SessionAbsoluteTimeoutSeconds);

        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {

            if (userData.Sessions == null)
            {
                userData.Sessions = new Dictionary<HashedSessionId, ActiveSession>(StructuralEqualityComparer<HashedSessionId>.Default);
            }

            if (!userData.Sessions.TryAdd(databaseCompatibleId, session))
            {
                return UserActionResult<SessionId>.Unsuccessful();
            }

            return UserActionResult<SessionId>.Successful(userCompatibleId);
        }

        return UserActionResult<SessionId>.UserNotFound();
    }
    public AuthorizationResult AuthenticateSession(SessionId sessionId)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                if (userData.Sessions.TryGetValue(parsedId.Value.databaseCompatibleId, out ActiveSession? session) && session != null)
                {
                    long currentTime = TimeUtils.GetUnixTime();

                    // Remove Session if expired and return false
                    if (!session.HasExpired(currentTime + Configuration.SessionClockSkewSeconds))
                    {
                        // Session is valid - refresh it and return
                        session.Refresh(currentTime + Configuration.SessionIdleTimeoutSeconds);
                        return AuthorizationResult.Authenticated(parsedId.Value.userId, userData.Username);
                    }

                    _ = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AuthorizeSession(SessionId sessionId, SinglePermission permission)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                if (userData.Sessions.TryGetValue(parsedId.Value.databaseCompatibleId, out ActiveSession? session) && session != null)
                {
                    long currentTime = TimeUtils.GetUnixTime();

                    // Remove Session if expired and return false
                    if (!session.HasExpired(currentTime + Configuration.SessionClockSkewSeconds))
                    {
                        // Session is valid - refresh it and return
                        session.Refresh(currentTime + Configuration.SessionIdleTimeoutSeconds);

                        if (userData.Permissions.Contains(permission))
                        {
                            return AuthorizationResult.Authorized(parsedId.Value.userId, userData.Username, permission);
                        }
                        else
                        {
                            return AuthorizationResult.Authenticated(parsedId.Value.userId, userData.Username);
                        }
                    }

                    _ = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AuthenticateStrictSession(SessionId sessionId, Password password)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                if (userData.Sessions.TryGetValue(parsedId.Value.databaseCompatibleId, out ActiveSession? session) && session != null)
                {
                    long currentTime = TimeUtils.GetUnixTime();

                    // Remove Session if expired and return false
                    if (!session.HasExpired(currentTime + Configuration.SessionClockSkewSeconds))
                    {
                        // Session is valid - refresh it and return
                        session.Refresh(currentTime + Configuration.SessionIdleTimeoutSeconds);

                        if (Configuration.PasswordHasher.Verify(password, userData.HashedPassword))
                        {
                            return AuthorizationResult.Authenticated(parsedId.Value.userId, userData.Username);
                        }

                    }

                    _ = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AuthorizeStrictSession(SessionId sessionId, Password password, SinglePermission permission)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.Sessions != null)
            {
                if (userData.Sessions.TryGetValue(parsedId.Value.databaseCompatibleId, out ActiveSession? session) && session != null)
                {
                    long currentTime = TimeUtils.GetUnixTime();

                    // Remove Session if expired and return false
                    if (!session.HasExpired(currentTime + Configuration.SessionClockSkewSeconds))
                    {
                        // Session is valid - refresh it and return
                        session.Refresh(currentTime + Configuration.SessionIdleTimeoutSeconds);

                        if (Configuration.PasswordHasher.Verify(password, userData.HashedPassword))
                        {
                            if (userData.Permissions.Contains(permission))
                            {
                                return AuthorizationResult.Authorized(parsedId.Value.userId, userData.Username, permission);
                            }
                            else
                            {
                                return AuthorizationResult.Authenticated(parsedId.Value.userId, userData.Username);
                            }
                        }

                    }

                    _ = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public UserActionResult RevokeSessionBySessionId(SessionId sessionId)
    {
        if (Configuration.SessionIdGenerator.TryParseId(sessionId, out (UserId userId, HashedSessionId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData != null)
            {
                if (userData.Sessions != null)
                {
                    bool removed = userData.Sessions.Remove(parsedId.Value.databaseCompatibleId);
                    if (userData.Sessions.Count <= 0)
                    {
                        userData.Sessions = null;
                    }

                    if (removed)
                    {
                        return UserActionResult.Successful();
                    }
                }

                return UserActionResult.Unsuccessful();
            }
        }

        return UserActionResult.UserNotFound();
    }
    public UserActionResult RevokeSessionBySessionInternalId(UserId userId, int internalSessionId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.Sessions != null)
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

                    if (removed)
                    {
                        return UserActionResult.Successful();
                    }
                }
            }

            return UserActionResult.Unsuccessful();
        }

        return UserActionResult.UserNotFound();
    }
    public UserActionResult RevokeAllSessions(UserId userId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.Sessions != null)
            {
                userData.Sessions.Clear();
                userData.Sessions = null;
                return UserActionResult.Successful();
            }
            return UserActionResult.Unsuccessful();
        }

        return UserActionResult.UserNotFound();
    }
    public UserActionResult<List<ActiveSession>> GetAllSessions(UserId userId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.Sessions != null)
            {
                return UserActionResult<List<ActiveSession>>.Successful([.. userData.Sessions.Values]);
            }

            return UserActionResult<List<ActiveSession>>.Unsuccessful();
        }

        return UserActionResult<List<ActiveSession>>.UserNotFound();
    }
    #endregion


    #region ApiKey
    public UserActionResult<ApiKeyId> CreateNewApiKey(UserId userId, Permissions permissions)
    {
        (string userCompatibleId, byte[] databaseCompatibleId) = Configuration.ApiKeyIdGenerator.GenerateId(userId);

        // Take the last 4 significant characters of the user compatible id
        string trimmed = userCompatibleId.TrimEnd('=');
        string internalKeyId = $"{trimmed[^4..]}{new HashedPassword('=', userCompatibleId.Length - trimmed.Length)}";

        long currentTime = TimeUtils.GetUnixTime();
        ApiKey apiKey = new ApiKey(currentTime, internalKeyId, permissions);
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.ApiKeys == null)
            {
                userData.ApiKeys = new Dictionary<HashedApiKeyId, ApiKey>(StructuralEqualityComparer<HashedApiKeyId>.Default);
            }

            if (!userData.ApiKeys.TryAdd(databaseCompatibleId, apiKey))
            {
                return UserActionResult<ApiKeyId>.Unsuccessful();
            }

            return UserActionResult<ApiKeyId>.Successful(userCompatibleId);
        }

        return UserActionResult<ApiKeyId>.UserNotFound();
    }
    public AuthorizationResult AuthenticateApiKey(ApiKeyId apiKeyId)
    {
        if (Configuration.ApiKeyIdGenerator.TryParseId(apiKeyId, out (UserId userId, HashedApiKeyId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.ApiKeys != null)
            {
                if (userData.ApiKeys.TryGetValue(parsedId.Value.databaseCompatibleId, out ApiKey? apiKey) && apiKey != null)
                {
                    long currentTime = TimeUtils.GetUnixTime();

                    apiKey.Use(currentTime);

                    return AuthorizationResult.Authenticated(parsedId.Value.userId, userData.Username);
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public AuthorizationResult AuthorizeApiKey(ApiKeyId apiKeyId, SinglePermission permission)
    {
        if (Configuration.ApiKeyIdGenerator.TryParseId(apiKeyId, out (UserId userId, HashedApiKeyId databaseCompatibleId)? parsedId) && parsedId.HasValue)
        {
            if (UserIdsToUserData.TryGetValue(parsedId.Value.userId, out UserData? userData) && userData?.ApiKeys != null)
            {
                if (userData.ApiKeys.TryGetValue(parsedId.Value.databaseCompatibleId, out ApiKey? apiKey) && apiKey != null)
                {
                    long currentTime = TimeUtils.GetUnixTime();

                    apiKey.Use(currentTime);

                    if (apiKey.Permissions.Contains(permission) && userData.Permissions.Contains(permission))
                    {
                        return AuthorizationResult.Authorized(parsedId.Value.userId, userData.Username, permission);
                    }
                    else
                    {
                        return AuthorizationResult.Authenticated(parsedId.Value.userId, userData.Username);
                    }
                }
            }
        }

        return AuthorizationResult.Failed();
    }
    public UserActionResult RevokeApiKeyPermissionByApiKeyInternalId(UserId userId, string internalId, SinglePermission permission)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.ApiKeys != null)
            {
                foreach ((_, ApiKey apiKey) in userData.ApiKeys)
                {
                    if (apiKey.InternalId == internalId)
                    {
                        bool removed = apiKey.Permissions.Remove(permission);
                        if (removed)
                        {
                            return UserActionResult.Successful();
                        }
                    }
                }
            }

            return UserActionResult.Unsuccessful();
        }

        return UserActionResult.UserNotFound();
    }
    public UserActionResult DeleteApiKeyByApiKeyInternalId(UserId userId, string internalId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.ApiKeys != null)
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
                    if (removed)
                    {
                        return UserActionResult.Successful();
                    }
                }
            }

            return UserActionResult.Unsuccessful();
        }

        return UserActionResult.UserNotFound();
    }
    public UserActionResult<List<ApiKey>> GetAllApiKeys(UserId userId)
    {
        if (UserIdsToUserData.TryGetValue(userId, out UserData? userData) && userData != null)
        {
            if (userData.ApiKeys != null)
            {
                return UserActionResult<List<ApiKey>>.Successful([.. userData.ApiKeys.Values]);
            }
            return UserActionResult<List<ApiKey>>.Unsuccessful();
        }
        return UserActionResult<List<ApiKey>>.UserNotFound();
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