using AuthProvider.Authentication;
using AuthProvider.CamInterface;
using AuthProvider.Exceptions;
using AuthProvider.RuntimePrecheck;
using AuthProvider.Swagger;
using AuthProvider.Utils;
using CredentialsAccessManager.CamInterface;
using CredentialsAccessManager.Controllers;
using CredentialsAccessManager.Credentials;
using CredentialsAccessManager.Credentials.CredentialStore;
using CredentialsAccessManager.Credentials.IdGenerators;
using CredentialsAccessManager.Credentials.PasswordHashing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace CredentialsAccessManager;

public class Program
{
    public async static Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplicationUtils.Initialize(args);

        var credentialStore = new CredentialStore(new()
        {
            DefaultUserPermissions = new Permissions(new() {
                { "cam", [Permission.LOGIN] }
            }),
            SessionIdleTimeoutSeconds = 20 * 60,
            SessionAbsoluteTimeoutSeconds = 500000,
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

        _ = builder.Services.AddSingleton(typeof(ICredentialStore), credentialStore);
        var camService = new LocalCamInterface("cam", credentialStore);
        _ = builder.Services.AddSingleton(typeof(ICamInterface), camService);

         await WebApplicationUtils.Start(builder);
    }
}
