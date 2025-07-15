using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

public static class PostgreTestContainer
{
    public static async Task<PostgreContainerSetup> InitializeAsync()
    {
        var container = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres123")
            .WithCleanUp(true)
            .Build();

        await container.StartAsync();

        var connection = new NpgsqlConnection(container.GetConnectionString());

        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseNpgsql(connection)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        await using var context = new MyDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new PostgreContainerSetup(container, options);
    }
}

public sealed class PostgreContainerSetup : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;
    public DbContextOptions<MyDbContext> Options { get; }

    public PostgreContainerSetup(PostgreSqlContainer container, DbContextOptions<MyDbContext> options)
    {
        _container = container;
        Options = options;
    }

    public string GetConnectionString() => _container.GetConnectionString();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
