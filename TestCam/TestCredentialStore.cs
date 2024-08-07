using AuthProvider.CamInterface;
using CredentialsAccessManager.Credentials;
using CredentialsAccessManager.Credentials.CredentialStore;
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
            SessionIdleTimeoutSeconds = 2, // 2 second idle timeout
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

        UserActionResult<Guid> created = CredentialStore.CreateUser(username, "some password");
        created.AssertSuccessfull();

        UserActionResult<Guid> getIdResult = CredentialStore.GetUserIdFromUsername(username);
        getIdResult.AssertSuccessfull();
        Assert.NotEqual(Guid.Empty, getIdResult.Output);

        UserActionResult<string> getUsernameResult = CredentialStore.GetUsernameFromUserId(getIdResult.Output);
        getUsernameResult.AssertSuccessfull(username);
    }

    [Fact]
    public void CreateSimpleUserWithSameUsername()
    {
        string username = "new user";

        UserActionResult<Guid> created = CredentialStore.CreateUser(username, "some password");
        created.AssertSuccessfull();

        created = CredentialStore.CreateUser(username, "some other password");
        created.AssertUnsuccessfull();
    }

    [Fact]
    public void CreateSimpleUserWithDistinctUsername()
    {
        UserActionResult<Guid> created = CredentialStore.CreateUser("new user", "some password");
        created.AssertSuccessfull();

        created = CredentialStore.CreateUser("some other new user", "some other password");
        created.AssertSuccessfull();
    }

    [Fact]
    public void CreateSimpleUserWithSameUsernameDifferentCase()
    {
        UserActionResult<Guid> created = CredentialStore.CreateUser("new user", "some password");
        created.AssertSuccessfull();

        created = CredentialStore.CreateUser("nEw User", "some other password");
        created.AssertUnsuccessfull();
    }

    [Fact]
    public void AuthenticateWithPasswordDifferentCase()
    {
        UserActionResult<Guid> created = CredentialStore.CreateUser("new user", "some password");
        created.AssertSuccessfull();

        AuthorizationResult result = CredentialStore.AuthenticateCredentials("neW user", "some password");

        UserActionResult<Guid> getIdResult = CredentialStore.GetUserIdFromUsername("new useR");
        getIdResult.AssertSuccessfull();
        result.AssertAuthenticated(getIdResult.Output);
    }

    [Fact]
    public void AuthorizeWithPasswordHasPermission()
    {
        UserActionResult<Guid> created = CredentialStore.CreateUser("new user", "some password");
        created.AssertSuccessfull();

        AuthorizationResult result = CredentialStore.AuthorizeCredentials("new user", "some password", (service_cam, permission_login));

        UserActionResult<Guid> getIdResult = CredentialStore.GetUserIdFromUsername("new user");

        getIdResult.AssertSuccessfull();
        result.AssertAuthorized(getIdResult.Output, (service_cam, permission_login));
    }

    [Fact]
    public void AuthorizeWithPasswordNoPermission()
    {
        UserActionResult<Guid> created = CredentialStore.CreateUser("new user", "some password");
        created.AssertSuccessfull();

        AuthorizationResult result = CredentialStore.AuthorizeCredentials("new user", "some password", (service_cam, "random_perm"));

        UserActionResult<Guid> getIdResult = CredentialStore.GetUserIdFromUsername("new user");
        getIdResult.AssertSuccessfull();
        result.AssertAuthenticated(getIdResult.Output);
    }

    [Fact]
    public void AuthorizeWithPasswordNoService()
    {
        UserActionResult<Guid> created = CredentialStore.CreateUser("new user", "some password");
        created.AssertSuccessfull();

        AuthorizationResult result = CredentialStore.AuthorizeCredentials("new user", "some password", ("no", permission_login));

        UserActionResult<Guid> getIdResult = CredentialStore.GetUserIdFromUsername("new user");
        getIdResult.AssertSuccessfull();
        result.AssertAuthenticated(getIdResult.Output);
    }

    [Fact]
    public void AuthenticateWithPasswordFail()
    {
        UserActionResult<Guid> created = CredentialStore.CreateUser("new user", "some password");
        created.AssertSuccessfull();

        AuthorizationResult result = CredentialStore.AuthenticateCredentials("new user", "some passwords");
        result.AssertAnonomyous();
    }

    [Fact]
    public void AuthorizeWithPasswordFail()
    {
        UserActionResult<Guid> created = CredentialStore.CreateUser("new user", "some password");
        created.AssertSuccessfull();

        AuthorizationResult result = CredentialStore.AuthorizeCredentials("new user", "some passwords", (service_cam, permission_login));
        result.AssertAnonomyous();
    }


}

internal static class UserActionResultExtensions
{
    internal static void AssertUserNotFound(this UserActionResult result)
    {
        Assert.False(result.FoundUser);
        Assert.False(result.OperationSuccess);
    }

    internal static void AssertUserNotFound<T>(this UserActionResult<T> result)
    {
        Assert.False(result.FoundUser);
        Assert.False(result.OperationSuccess);
        Assert.Null(result.Output);
    }

    internal static void AssertUnsuccessfull(this UserActionResult result)
    {
        Assert.True(result.FoundUser);
        Assert.False(result.OperationSuccess);
    }

    internal static void AssertUnsuccessfull<T>(this UserActionResult<T> result)
    {
        Assert.True(result.FoundUser);
        Assert.False(result.OperationSuccess);
        Assert.Equal(default, result.Output);
    }

    internal static void AssertSuccessfull(this UserActionResult result)
    {
        Assert.True(result.FoundUser);
        Assert.True(result.OperationSuccess);
    }

    internal static void AssertSuccessfull<T>(this UserActionResult<T> result)
    {
        Assert.True(result.FoundUser);
        Assert.True(result.OperationSuccess);
        Assert.NotNull(result.Output);
    }

    internal static void AssertSuccessfull<T>(this UserActionResult<T> result, T expected)
    {
        Assert.True(result.FoundUser);
        Assert.True(result.OperationSuccess);
        Assert.Equal(expected, result.Output);
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
        _ = Assert.NotNull(result.Permission);
        Assert.Equal(userId, result.UserId);
    }

    internal static void AssertAuthorized(this AuthorizationResult result, (string service, string permission) permission)
    {
        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsAuthorized);
        _ = Assert.NotNull(result.UserId);
        Assert.Equal(permission, result.Permission);
    }

    internal static void AssertAuthorized(this AuthorizationResult result)
    {
        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsAuthorized);
        _ = Assert.NotNull(result.UserId);
        _ = Assert.NotNull(result.Permission);
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
        _ = Assert.NotNull(result.UserId);
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