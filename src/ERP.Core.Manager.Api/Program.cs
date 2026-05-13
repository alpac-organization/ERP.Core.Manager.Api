using DotNetEnv;
using System.Text.Json;
using Microsoft.OpenApi;
using ERP.Core.Manager.Api.Application;
using ERP.Core.Manager.Api.Infrastructure;
using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Infrastructure.Middlewares;
using ERP.Core.Infrastructure.Middlewares;


var builder = WebApplication.CreateBuilder(args);

var root = builder.Environment.ContentRootPath;

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteLocalPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5174", "http://localhost:5173", "https://web-alpac.onrender.com")
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
