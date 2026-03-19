using System.Text.Json;
using Microsoft.OpenApi;
using ERP.Core.Manager.Api.Application;
using ERP.Core.Manager.Api.Infrastructure;
using System.Text.Json.Serialization;
using ERP.Core.Manager.Api.Infrastructure.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteLocalPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173") // Puertos comunes de Vite
            .AllowAnyMethod()
            .AllowAnyHeader();
            //   .AllowCredentials()
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

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("ViteLocalPolicy");
app.UseMiddleware<ApiKeyMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger/docs";
    });
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
