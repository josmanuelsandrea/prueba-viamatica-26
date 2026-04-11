using Microsoft.EntityFrameworkCore;
using Serilog;
using ViamaticaApi.Api.Extensions;
using ViamaticaApi.Api.Middlewares;
using ViamaticaApi.Infrastructure.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/viamatica-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30
            )
    );

    builder.Services.AddControllers();
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddRedis(builder.Configuration);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(
                    builder.Configuration.GetValue<string>("Cors:FrontendUrl") ?? "http://localhost:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // Verificar conexión a la base de datos al iniciar
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var encryption = scope.ServiceProvider.GetRequiredService<ViamaticaApi.Application.Interfaces.IEncryptionService>();
        try
        {
            Log.Information("Verificando conexión a la base de datos...");
            await db.Database.OpenConnectionAsync();
            await db.Database.CloseConnectionAsync();
            Log.Information("Conexión a la base de datos establecida correctamente.");

            // Seed: encriptar credenciales de usuarios iniciales si están pendientes
            var seedUsers = new[]
            {
                (Username: "admin001",  Password: "Admin1234",   Email: "admin@viamatica.com"),
                (Username: "gestor001", Password: "Gestor1234",  Email: "gestor@viamatica.com"),
                (Username: "cajero001", Password: "Cajero1234",  Email: "cajero@viamatica.com"),
            };

            foreach (var seed in seedUsers)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == seed.Username);
                if (user != null && user.Password == "PENDING_ENCRYPTION")
                {
                    user.Password = encryption.Encrypt(seed.Password);
                    user.Email = encryption.Encrypt(seed.Email);
                    Log.Information("Credenciales del usuario {Username} configuradas correctamente.", seed.Username);
                }
            }
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "No se pudo conectar a la base de datos. Verifique la cadena de conexión y que el servidor esté disponible.");
            throw;
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Viamatica API v1"));

    app.UseSerilogRequestLogging();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseWhen
    (ctx =>
        ctx.Request.Path.StartsWithSegments("/api/auth") ||
        ctx.Request.Path.StartsWithSegments("/api/contracts") ||
        ctx.Request.Path.StartsWithSegments("/api/payments"),
    appBuilder =>
    {
        appBuilder.UseMiddleware<AuditMiddleware>();
    });
    app.MapControllers();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var urls = string.Join(" | ", app.Urls);
        Log.Information("Viamatica API corriendo en: {Urls}", urls);
        Log.Information("Swagger disponible en: {SwaggerUrl}", app.Urls.FirstOrDefault() + "/swagger");
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
