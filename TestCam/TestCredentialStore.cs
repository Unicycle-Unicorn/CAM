using CredentialsAccessManager;
using CredentialsAccessManager.Credentials;
using CredentialsAccessManager.Credentials.IdGenerators;
using CredentialsAccessManager.Credentials.PasswordHashing;

namespace TestCam;

public class TestCredentialStore
{
    public ICredentialStore CredentialStore;

    const string service_cam = "CAM";
    const string permission_login = "login";
    const string permission_write = "write";

    public TestCredentialStore()
    {
        CredentialStore = new CredentialStore(new()
        {
            DefaultUserPermissions = new Permissions(new() {
                { service_cam, [permission_login, permission_write] }
            }),
            SessionIdleTimeoutSeconds = 5, // 2 second idle timeout
            SessionAbsoluteTimeoutSeconds = 5, // 5 second absolute timeout
            PasswordHasher = new PasswordHasher(new()),
            ApiKeyIdGenerator = new IdGenerator(new()
            {
                IdLengthBytes = 12,
                Hasher = System.Security.Cryptography.SHA256.HashData
            }),
            SessionIdGenerator = new IdGenerator(new()
            {
                IdLengthBytes = 8
            })
        });
    }


    [Fact]
    public void CreateSimpleUser()
    {
        string username = "new user";

        bool created = CredentialStore.TryCreateUser(username, "some password");
        Assert.True(created);

        bool gotUserId = CredentialStore.TryGetUserIdFromUsername(username, out Guid userId);
        Assert.True(gotUserId);
        Assert.NotEqual(Guid.Empty, userId);

        bool gotUsername = CredentialStore.TryGetUsernameFromUserId(userId, out string? recievedUsername);
        Assert.True(gotUsername);
        Assert.NotNull(recievedUsername);

        Assert.Equal(username, recievedUsername);
    }

    [Fact]
    public void CreateSimpleUserWithSameUsername()
    {
        string username = "new user";

        bool created = CredentialStore.TryCreateUser(username, "some password");
        Assert.True(created);

        created = CredentialStore.TryCreateUser(username, "some other password");
        Assert.False(created);
    }

    [Fact]
    public void CreateSimpleUserWithDistinctUsername()
    {
        bool created = CredentialStore.TryCreateUser("new user", "some password");
        Assert.True(created);

        created = CredentialStore.TryCreateUser("some other new user", "some other password");
        Assert.True(created);
    }

    [Fact]
    public void CreateSimpleUserWithSameUsernameDifferentCase()
    {
        bool created = CredentialStore.TryCreateUser("new user", "some password");
        Assert.True(created);

        created = CredentialStore.TryCreateUser("nEw User", "some other password");
        Assert.False(created);
    }

    [Fact]
    public void AuthenticateWithPasswordDifferentCase()
    {
        bool created = CredentialStore.TryCreateUser("new user", "some password");
        Assert.True(created);

        AuthorizationResult result = CredentialStore.AttemptAuthenticateCredentials("neW user", "some password");

        _ = CredentialStore.TryGetUserIdFromUsername("new useR", out Guid userId);
        result.AssertAuthenticated(userId);
    }

    [Fact]
    public void AuthorizeWithPasswordHasPermission()
    {
        bool created = CredentialStore.TryCreateUser("new user", "some password");
        Assert.True(created);

        AuthorizationResult result = CredentialStore.AttemptAuthorizeCredentials("new user", "some password", (service_cam, permission_login));

        _ = CredentialStore.TryGetUserIdFromUsername("new user", out Guid userId);
        result.AssertAuthorized(userId, (service_cam, permission_login));
    }

    [Fact]
    public void AuthorizeWithPasswordNoPermission()
    {
        bool created = CredentialStore.TryCreateUser("new user", "some password");
        Assert.True(created);

        AuthorizationResult result = CredentialStore.AttemptAuthorizeCredentials("new user", "some password", (service_cam, "random_perm"));
        
        _ = CredentialStore.TryGetUserIdFromUsername("new user", out Guid userId);
        result.AssertAuthenticated(userId);
    }

    [Fact]
    public void AuthorizeWithPasswordNoService()
    {
        bool created = CredentialStore.TryCreateUser("new user", "some password");
        Assert.True(created);

        AuthorizationResult result = CredentialStore.AttemptAuthorizeCredentials("new user", "some password", ("no", permission_login));
        
        _ = CredentialStore.TryGetUserIdFromUsername("new user", out Guid userId);
        result.AssertAuthenticated(userId);
    }

    [Fact]
    public void AuthenticateWithPasswordFail()
    {
        bool created = CredentialStore.TryCreateUser("new user", "some password");
        Assert.True(created);

        AuthorizationResult result = CredentialStore.AttemptAuthenticateCredentials("new user", "some passwords");
        result.AssertAnonomyous();
    }

    [Fact]
    public void AuthorizeWithPasswordFail()
    {
        bool created = CredentialStore.TryCreateUser("new user", "some password");
        Assert.True(created);
        
        AuthorizationResult result = CredentialStore.AttemptAuthorizeCredentials("new user", "some passwords", (service_cam, permission_login));
        result.AssertAnonomyous();
    }

    
}

internal static class AuthorizationResultExtensions
{
    internal static void AssertAuthorized(this AuthorizationResult result, Guid userId, (string service, string permission) permission)
    {
        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsAuthorized);
        Assert.Equal(permission, result.Permission);
        Assert.Equal(userId, result.UserId);
    }

    internal static void AssertAuthorized(this AuthorizationResult result, Guid userId)
    {
        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsAuthorized);
        Assert.NotNull(result.Permission);
        Assert.Equal(userId, result.UserId);
    }

    internal static void AssertAuthorized(this AuthorizationResult result, (string service, string permission) permission)
    {
        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsAuthorized);
        Assert.NotNull(result.UserId);
        Assert.Equal(permission, result.Permission);
    }

    internal static void AssertAuthorized(this AuthorizationResult result)
    {
        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsAuthorized);
        Assert.NotNull(result.UserId);
        Assert.NotNull(result.Permission);
    }


    internal static void AssertAuthenticated(this AuthorizationResult result, Guid userId)
    {
        Assert.True(result.IsAuthenticated);
        Assert.False(result.IsAuthorized);
        Assert.Equal(userId, result.UserId);
        Assert.Null(result.Permission);
    }

    internal static void AssertAuthenticated(this AuthorizationResult result)
    {
        Assert.True(result.IsAuthenticated);
        Assert.False(result.IsAuthorized);
        Assert.NotNull(result.UserId);
        Assert.Null(result.Permission);
    }


    internal static void AssertAnonomyous(this AuthorizationResult result)
    {
        Assert.False(result.IsAuthenticated);
        Assert.False(result.IsAuthorized);
        Assert.Null(result.UserId);
        Assert.Null(result.Permission);
    }
}