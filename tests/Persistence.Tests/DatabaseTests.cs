using Dapper;
using Testcontainers.PostgreSql;

// ReSharper disable InconsistentNaming

namespace Persistence.Tests;

public class DatabaseTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public DatabaseTests()
    {
        var builder = new PostgreSqlBuilder()
            .WithImage("postgres:latest");

        _container = builder.Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        DapperDatabase.Initialize(new DapperConnectionFactory(_container.GetConnectionString()));
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task PostgresVersion_ReturnsVersion()
    {
        // Arrange
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        // Act
        var result = await db.QueryFirstAsync<string>("SELECT VERSION();");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task DatabaseInitializeCheck_ReturnsCountOfTables()
    {
        // Arrange
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        // Act
        var result = await db.QueryAsync<string>("SELECT tablename FROM pg_tables WHERE schemaname = 'public';");

        // Assert
        Assert.Equal(5, result.Count());
    }
}