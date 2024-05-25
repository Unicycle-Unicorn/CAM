using AuthProvider;
using AuthProvider.Authentication;
using AuthProvider.CamInterface;
using AuthProvider.RuntimePrecheck;
using AuthProvider.Swagger;
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

        _ = builder.Services.AddSwaggerGen(config => config.OperationFilter<SwaggerAuth>());

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
        
        _ = builder.Services.AddLogging();

        

        WebApplication app = builder.Build();
        
        // First thing we want to do is run a check of all actions everywhere to find possible issues
        RuntimePrechecker.RunPrecheck(app);

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
