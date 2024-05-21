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
    public bool TryCreateUser(Username username, Password password);
    public AuthorizationResult AttemptAuthenticateCredentials(Username username, Password password);
    public AuthorizationResult AttemptAuthorizeCredentials(Username username, Password password, SinglePermission permission);
    public bool TryGetUserIdFromUsername(Username username, [NotNullWhen(true)] out UserId userId);
    public bool TryGetUsernameFromUserId(UserId userId, [NotNullWhen(true)] out Username? username);
    // public bool TryGetUserPermissions(UserId userId, [NotNullWhen(true)] out Permissions? username);
    // public bool GrantUserPermission(UserId userId, Permission permission);
    // public bool RevokeUserPermission(UserId userId, Permission permission);
    // public bool TryGetUserInformation(UserId userId, [NotNullWhen(true)] out UserInformation userInformation);
    // public bool UpdateUserInformation(UserId userId, UserInformation newUserInformation);
    // public bool UpdateUsername(UserId userId, Username username);
    // public bool UpdatePassword(UserId userId, Password password);
    #endregion


    #region Session
    public bool TryCreateNewSession(UserId userId, [NotNullWhen(true)] out SessionId? sessionId);
    public AuthorizationResult AttemptAuthenticateSession(SessionId sessionId);
    public AuthorizationResult AttemptAuthorizeSession(SessionId sessionId, SinglePermission permission);
    public AuthorizationResult AttemptAuthenticateStrictSession(SessionId sessionId, Password password);
    public AuthorizationResult AttemptAuthorizeStrictSession(SessionId sessionId, Password password, SinglePermission permission);
    public bool RevokeSessionBySessionId(SessionId sessionId);
    public bool RevokeSessionBySessionInternalId(UserId userId, int internalSessionId);
    public bool RevokeAllSessions(UserId userId);
    public bool TryGetAllSessions(UserId userId, [NotNullWhen(true)] out List<ActiveSession>? sessions);
    #endregion


    #region ApiKey
    public bool TryCreateNewApiKey(UserId userId, Permissions permissions, [NotNullWhen(true)] out ApiKeyId? apiKeyid);
    public AuthorizationResult AttemptAuthenticateApiKey(ApiKeyId apiKeyId);
    public AuthorizationResult AttemptAuthorizeApiKey(ApiKeyId apiKeyId, SinglePermission permission);
    public bool RevokeApiKeyPermissionByApiKeyInternalId(UserId userId, string internalId, SinglePermission permission);
    public bool DeleteApiKeyByApiKeyInternalId(UserId userId, string internalId);
    public bool TryGetAllApiKeys(UserId userId, [NotNullWhen(true)] out List<ApiKey>? apiKeys);
    #endregion
}