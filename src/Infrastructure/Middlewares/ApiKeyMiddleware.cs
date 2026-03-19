using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ERP.Core.Manager.Api.Infrastructure.Middlewares
{
    public class ApiKeyMiddleware(RequestDelegate _next, IConfiguration _config)
    {
        private const string APIKEYNAME = "x-api-key";

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Intentamos obtener la API Key de los Headers
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                await HandleUnauthorizedAsync(context, "API Key no proporcionada en los headers.");
                return;
            }

            // 2. Obtenemos la llave válida desde las variables de entorno (appsettings.json)
            var apiKey = _config["Authentication:ApiKey"];

            // 3. Validamos si coinciden
            if (string.IsNullOrEmpty(apiKey) || !apiKey.Equals(extractedApiKey))
            {
                await HandleUnauthorizedAsync(context, "Acceso denegado.");
                return;
            }

            // Si todo está bien, pasamos al siguiente middleware (Exception, Auth, etc.)
            await _next(context);
        }

        private static Task HandleUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 403; // Forbidden o 401 Unauthorized

            var response = new
            {
                Status = 403,
                Error = new { TypeError = "Forbidden", Description = message },
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var options = new System.Text.Json.JsonSerializerOptions { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
            };

            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, options));
        }
    }
}