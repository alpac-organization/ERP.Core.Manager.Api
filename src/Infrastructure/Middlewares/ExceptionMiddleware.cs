using System.Text.Json;
using ERP.Core.Manager.Api.Domain.Entities.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ERP.Core.Manager.Api.Infrastructure.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate _next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (CoreException ex)
            {
                // Aquí capturamos tu ErrorResponse personalizado
                await HandleExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                // Error genérico para cosas que no controlamos (500)
                await HandleInternalExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, CoreException exception)
        {
            context.Response.ContentType = "application/json";
            
            // CAMBIO: Cambia .ErrorResponse por .ErrorData
            context.Response.StatusCode = exception.ErrorData.Status;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            
            // CAMBIO: Aquí también usa .ErrorData
            var result = JsonSerializer.Serialize(exception.ErrorData, options);

            return context.Response.WriteAsync(result);
        }

        private static Task HandleInternalExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            // Creamos un ErrorResponse manual para errores de servidor
            var response = new { 
                Status = 500, 
                Error = new { TypeError = "Server_Error", Description = "Error interno no controlado." },
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}