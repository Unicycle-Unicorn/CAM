
using AuthProvider;
using CredentialsAccessManager.Session;
using CredentialsAccessManager.User;

namespace CredentialsAccessManager;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddAuthentication(NullAuthenticationHandler.RegisterWithBuilder);

        builder.Services.AddSingleton(typeof(IUserStore), new UserStore());
        builder.Services.AddSingleton(typeof(ISessionStore), new SessionStore());

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        RegistrationService.RegisterPermission(Permission.LOGIN);
        RegistrationService.RegisterService("CAM");

        app.Run();
    }
}
