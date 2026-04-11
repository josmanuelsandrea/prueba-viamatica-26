using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Middlewares
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var encryption = context.RequestServices.GetRequiredService<IEncryptionService>();

            var method = context.Request.Method;
            var path = context.Request.Path;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

            var username = "anonymous";

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var rawClaim = context.User.FindFirst("username")?.Value;
                if (rawClaim != null)
                {
                    username = encryption.Decrypt(rawClaim);
                }
            }

            await _next(context);
            var statusCode = context.Response.StatusCode;

            _logger.LogInformation("AUDIT | {Method} {Path} | User: {Username} | IP: {Ip} | Status {StatusCode}", method, path, username, ip, statusCode);
        }
    }
}
