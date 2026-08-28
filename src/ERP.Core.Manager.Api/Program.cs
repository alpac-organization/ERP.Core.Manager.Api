using System.Text.Json;
using Microsoft.OpenApi;
using ERP.Core.Manager.Api.Application;
using ERP.Core.Manager.Api.Infrastructure;
using System.Text.Json.Serialization;
using ERP.Core.Infrastructure.Middlewares;
using System.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

var root = builder.Environment.ContentRootPath;
var envPath = Path.Combine(root, "..", "..", ".env");

if (File.Exists(envPath)) DotNetEnv.Env.NoClobber().Load(envPath);
else DotNetEnv.Env.NoClobber().Load();

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

// Configuración correcta de Kestrel para forzar TLS 1.2
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls12;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteLocalPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? [])
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
});

builder.Logging.AddFilter("LuckyPennySoftware.MediatR.License", LogLevel.None);

//Configuración de la documentacion del swagger de las APIs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERP.Core.Manager.Api",
        Version = "v1",
        Description = "Dominio para administración "
    });
});

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
    }
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();

app.UseCors("ViteLocalPolicy");

app.UseMiddleware<ApiKeyMiddleware>();

app.UseMiddleware<AuthMiddleware>();    


if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger/docs";
    });
}

app.UseHsts();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();


app.Run();
