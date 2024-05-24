
using AuthProvider;
using AuthProvider.Authentication;
using AuthProvider.CamInterface;
using CredentialsAccessManager.CamInterface;
using CredentialsAccessManager.Credentials;
using CredentialsAccessManager.Credentials.CredentialStore;
using CredentialsAccessManager.Credentials.IdGenerators;
using CredentialsAccessManager.Credentials.PasswordHashing;

namespace CredentialsAccessManager;

public class Program
{
    public async static Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        _ = builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = builder.Services.AddEndpointsApiExplorer();

        _ = builder.Services.AddSwaggerGen(config => config.OperationFilter<AuthAttribute<CredentialAuth>>());
        _ = builder.Services.AddSwaggerGen(config => config.OperationFilter<AuthAttribute<SessionAuth>>());
        
        _ = builder.Services.AddAuthentication(NullAuthenticationHandler.RegisterWithBuilder);

        //builder.Services.AddSingleton(typeof(ICamInterface), new RemoteCamInterface("cam", "https://api.unicycleunicorn.net/cam"));
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

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI();
        }

        _ = app.UseAuthorization();

        _ = app.MapControllers();

        await camService.Initialize();

        app.Run();
    }
}
