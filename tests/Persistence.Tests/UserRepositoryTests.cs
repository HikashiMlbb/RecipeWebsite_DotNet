using Dapper;
using Domain.UserEntity;
using Infrastructure.Services;
using Persistence.Repositories;
using Testcontainers.PostgreSql;
// ReSharper disable InconsistentNaming

namespace Persistence.Tests;

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private UserRepository _repository = null!;

    public UserRepositoryTests()
    {
        var builder = new PostgreSqlBuilder()
            .WithImage("postgres:latest");

        _container = builder.Build();
    }

    [Fact]
    public async Task InsertingUniqueUser_ReturnsUserId()
    {
        // Arrange
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();
        var passwordService = new PasswordService();
        var user = new User(Username.Create("JustAUsername").Value!, await passwordService.CreateAsync("SomeSomePassword!"));

        // Act
        var result = await _repository.InsertAsync(user);
        var custom = await db.QueryFirstAsync<int>("SELECT COUNT(*) FROM Users;");

        // Assert
        Assert.True(custom == 1);
        Assert.True(result.Value != 0);
    }
    
    [Fact]
    public async Task InsertingNotUniqueUser_ReturnsNone()
    {
        // Arrange
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();
        var passwordService = new PasswordService();
        var user = new User(Username.Create("JustAUsername").Value!, await passwordService.CreateAsync("SomeSomePassword!"));

        // Act
        var resultSuccess = await _repository.InsertAsync(user);
        var resultFailure = await _repository.InsertAsync(user);
        var custom = await db.QueryFirstAsync<int>("SELECT COUNT(*) FROM Users;");

        // Assert
        Assert.True(custom == 1);
        Assert.True(resultSuccess.Value != 0);
        Assert.True(resultFailure.Value == 0);
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
}