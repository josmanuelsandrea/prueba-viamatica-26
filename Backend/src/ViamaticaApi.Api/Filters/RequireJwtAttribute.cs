using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Filters
{
    public class RequireJwtAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var encryption = context.HttpContext.RequestServices.GetRequiredService<IEncryptionService>();

            var header = context.HttpContext.Request.Headers["Authorization"];
            // Verify if header starts with "Bearer " and token is not empty

            var headerVerification = header.ToString().StartsWith("Bearer ") && !string.IsNullOrEmpty(header.ToString().Substring("Bearer ".Length).Trim());
            if (!headerVerification) // If the header is not valid, return 401 Unauthorized
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Token requerido"});
                return;
            }

            var token = header.ToString().Substring("Bearer ".Length).Trim();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                ValidateIssuer = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = config["Jwt:Audience"],
                ValidateLifetime = true,
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, validationParams, out SecurityToken validatedToken);
                await next();
            } catch (Exception)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Token inválido o expirado"});
            }
        }
    }
}
