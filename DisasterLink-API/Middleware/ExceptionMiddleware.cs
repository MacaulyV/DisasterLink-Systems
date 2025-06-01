using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DisasterLink_API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                var status = ex switch
                {
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    ApplicationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };
                context.Response.StatusCode = status;
                var error = ex switch
                {
                    KeyNotFoundException => new { errors = new { message = ex.Message } },
                    ApplicationException => new { errors = new { message = ex.Message } },
                    _ => new { errors = new { message = "Ocorreu um erro interno no servidor." } }
                };
                var result = JsonSerializer.Serialize(error);
                await context.Response.WriteAsync(result);
            }
        }
    }
} 