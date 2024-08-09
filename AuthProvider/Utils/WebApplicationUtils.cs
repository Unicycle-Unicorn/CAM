

using AuthProvider.Authentication;
using AuthProvider.CamInterface;
using AuthProvider.Exceptions;
using AuthProvider.RuntimePrecheck;
using AuthProvider.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthProvider.Utils;


public static class WebApplicationUtils
{
    const string CorsAllowAll = "CorsAllowAll";

    public static WebApplicationBuilder Initialize(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen(config => config.OperationFilter<SwaggerAuth>());

        _ = builder.Services.AddAuthentication(NullAuthenticationHandler.RegisterWithBuilder);

        _ = builder.Services.AddExceptionHandler<UuidExceptionHandler>();
        _ = builder.Services.AddProblemDetails();

        if (builder.Environment.IsDevelopment()) {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsAllowAll, builder =>
                {
                    _ = builder.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader().AllowCredentials().WithExposedHeaders(HeaderUtils.XExceptionCode);
                });
            });
        }
        

        return builder;
    }

    public static void AddRemoteCam(WebApplicationBuilder builder, string service)
    {

        ICamInterface camInterface;
        if (builder.Environment.IsDevelopment()) {
            camInterface = new RemoteCamInterface(service, "http://localhost:5048");
        }
        else
        {
            camInterface = new RemoteCamInterface(service, "http://cam:8080");
        }
        
        
        _ = builder.Services.AddSingleton(typeof(ICamInterface), camInterface);
    }

    public async static Task Start(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        RuntimePrechecker.RunPrecheck(app);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // We don't need https because our services are secured via nginx https through a proxy pass over http to this service
        // app.UseHttpsRedirection();

        _ = app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            _ = app.UseCors(CorsAllowAll);
        }

        _ = app.UseExceptionHandler();
        _ = app.MapControllers();

        await app.Services.GetService<ICamInterface>().Initialize();

        app.Run();
    }
}
