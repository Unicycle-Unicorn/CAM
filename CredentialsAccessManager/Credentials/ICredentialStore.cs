using UserId = System.Guid;
using Username = string;
using Password = string;
using ApiKeyId = string;
using SessionId = string;
using SinglePermission = (string service, string permission);

using System.Diagnostics.CodeAnalysis;
using CredentialsAccessManager.Session;

namespace CredentialsAccessManager.Credentials;

public interface ICredentialStore
{
    #region User
    public bool CreateUser(Username username, Password password);
    public AuthorizationResult AuthenticateCredentials(Username username, Password password);
    public AuthorizationResult AuthorizeCredentials(Username username, Password password, SinglePermission permission);
    public UserActionResult<UserId> GetUserIdFromUsername(Username username);
    public UserActionResult<Username> GetUsernameFromUserId(UserId userId);

    // public bool GetUserPermissions(UserId userId, [NotNullWhen(true)] out Permissions? username);
    // public bool GrantUserPermission(UserId userId, Permission permission);
    // public bool RevokeUserPermission(UserId userId, Permission permission);
    // public bool GetUserInformation(UserId userId, [NotNullWhen(true)] out UserInformation userInformation);
    // public bool UpdateUserInformation(UserId userId, UserInformation newUserInformation);
    // public bool UpdateUsername(UserId userId, Username username);
    // public bool UpdatePassword(UserId userId, Password password);
    #endregion


    #region Session
    public bool CreateNewSession(UserId userId, [NotNullWhen(true)] out SessionId? sessionId);
    public AuthorizationResult AuthenticateSession(SessionId sessionId);
    public AuthorizationResult AuthorizeSession(SessionId sessionId, SinglePermission permission);
    public AuthorizationResult AuthenticateStrictSession(SessionId sessionId, Password password);
    public AuthorizationResult AuthorizeStrictSession(SessionId sessionId, Password password, SinglePermission permission);
    public bool RevokeSessionBySessionId(SessionId sessionId);
    public bool RevokeSessionBySessionInternalId(UserId userId, int internalSessionId);
    public bool RevokeAllSessions(UserId userId);
    public bool GetAllSessions(UserId userId, [NotNullWhen(true)] out List<ActiveSession>? sessions);
    #endregion


    #region ApiKey
    public bool CreateNewApiKey(UserId userId, Permissions permissions, [NotNullWhen(true)] out ApiKeyId? apiKeyid);
    public AuthorizationResult AuthenticateApiKey(ApiKeyId apiKeyId);
    public AuthorizationResult AuthorizeApiKey(ApiKeyId apiKeyId, SinglePermission permission);
    public bool RevokeApiKeyPermissionByApiKeyInternalId(UserId userId, string internalId, SinglePermission permission);
    public bool DeleteApiKeyByApiKeyInternalId(UserId userId, string internalId);
    public bool GetAllApiKeys(UserId userId, [NotNullWhen(true)] out List<ApiKey>? apiKeys);
    #endregion
}