using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Infrastructure.Data;
using ViamaticaApi.Infrastructure.Services;

namespace ViamaticaApi.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ICashService, CashService>();
        services.AddScoped<ITurnService, TurnService>();
        services.AddScoped<IAttentionService, AttentionService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IKioskService, KioskService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]);
        services.AddSingleton<IConnectionMultiplexer>(connection);

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Viamatica API",
                Version = "v1",
                Description = "API REST para sistema de gestión de cajas - Viamatica"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
