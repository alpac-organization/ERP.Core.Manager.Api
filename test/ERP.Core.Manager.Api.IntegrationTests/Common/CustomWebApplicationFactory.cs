using System.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Respawn;
using Testcontainers.PostgreSql;

using ERP.Core.Application.Commons.Interfaces.AWS;
using ERP.Core.Database.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.IntegrationTests.Common.Services;

namespace ERP.Core.Manager.Api.IntegrationTests.Common;


public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string ApiKey = "integration-test-api-key";
    public const string JwtKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCqGKukO1De7zhY";
    private const string DummyClockConnection =
        "Server=localhost,1433;Database=Marcaciones;User Id=test;Password=test;TrustServerCertificate=True";

    private PostgreSqlContainer? _container;
    private bool _initialized;

    /// <summary>Indica si el motor Docker está disponible y el contenedor pudo levantarse.</summary>
    public bool IsDockerAvailable { get; private set; }

    /// <summary>Motivo por el cual el contenedor no pudo levantarse (para diagnóstico).</summary>
    public string? UnavailableReason { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json"),
                optional: true,
                reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            var snsDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISimpleNotificationServices));
            if (snsDescriptor is not null)
            {
                services.Remove(snsDescriptor);
            }

            services.AddScoped<ISimpleNotificationServices, FakeSimpleNotificationServices>();
        });
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            _container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("erp_test")
                .WithUsername("erp_test_user")
                .WithPassword("erp_test_password")
                .Build();

            await _container.StartAsync();

            SetEnvironmentVariables(_container.GetConnectionString());

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            IsDockerAvailable = true;
        }
        catch (Exception ex)
        {
            IsDockerAvailable = false;
            UnavailableReason = string.Join(" ", ex.Message.Split('\n')[0]);
        }
        finally
        {
            _initialized = true;
        }
    }

    public async Task ResetDatabaseAsync()

    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            WithReseed = false
        });

        await respawner.ResetAsync(connection);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _container?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _container = null;
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
            _container = null;
        }

        await base.DisposeAsync();
    }

    private static void SetEnvironmentVariables(string connectionString)
    {
        // Datos que la API lee durante la inicialización (AddInfrastructureServices).
        Environment.SetEnvironmentVariable("ConnectionStrings__ErpConnectionDatabase", connectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__ClockConnectionDatabase", DummyClockConnection);
        Environment.SetEnvironmentVariable("Authentication__ApiKey", ApiKey);
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins__0", "http://localhost");

        // Dependencias externas (AWS S3 / SNS) — valores dummy para evitar llamadas de red.
        Environment.SetEnvironmentVariable("S3Storage__AccessKey", "test");
        Environment.SetEnvironmentVariable("S3Storage__SecretKey", "test");
        Environment.SetEnvironmentVariable("S3Storage__Region", "us-east-1");
        Environment.SetEnvironmentVariable("S3Storage__BucketName", "erp-test");
        Environment.SetEnvironmentVariable("S3Storage__ForcePathStyle", "true");
        Environment.SetEnvironmentVariable("S3Storage__PublicKeyBaseUrl", "http://localhost");
        Environment.SetEnvironmentVariable("S3Storage__ServiceUrl", "");

        Environment.SetEnvironmentVariable("AwsSns__AccessKey", "test");
        Environment.SetEnvironmentVariable("AwsSns__SecretKey", "test");
        Environment.SetEnvironmentVariable("AwsSns__Region", "us-east-1");
        Environment.SetEnvironmentVariable("AwsSns__PlatformApplicationArn", "arn:aws:sns:us-east-1:000000000000:app/GCM/test");
        Environment.SetEnvironmentVariable("AwsSns__DefaultTopicArn", "");
    }
}
