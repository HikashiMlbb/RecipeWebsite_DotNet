using Dapper;
using Domain.RecipeEntity;
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
    public async Task InsertingUniqueUser_ReturnsUserId()
    {
        // Arrange
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();
        var passwordService = new PasswordService();
        var user = new User(Username.Create("JustAUsername").Value!,
            await passwordService.CreateAsync("SomeSomePassword!"));

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
        var user = new User(Username.Create("JustAUsername").Value!,
            await passwordService.CreateAsync("SomeSomePassword!"));

        // Act
        var resultSuccess = await _repository.InsertAsync(user);
        var resultFailure = await _repository.InsertAsync(user);
        var custom = await db.QueryFirstAsync<int>("SELECT COUNT(*) FROM Users;");

        // Assert
        Assert.True(custom == 1);
        Assert.True(resultSuccess.Value != 0);
        Assert.True(resultFailure.Value == 0);
    }

    [Fact]
    public async Task SearchByUsername_NotFound_ReturnsNull()
    {
        // Arrange
        var username = Username.Create("Vovan").Value!;
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));

        // Act
        var result = await _repository.SearchByUsernameAsync(username);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchByUsername_Found_ReturnsUser()
    {
        // Arrange
        var id = new UserId(5);
        var username = Username.Create("Vovan").Value!;
        var password = new Password("$omeHa$hedPa$$word");
        var user = new User { Id = id, Username = username, Password = password };
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();
        await db.ExecuteAsync("INSERT INTO Users VALUES (5, @username, @password, 'classic')", new
        {
            username = username.Value,
            password = password.PasswordHash
        });

        // Act
        var result = await _repository.SearchByUsernameAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(password, result.Password);
    }

    [Fact]
    public async Task SearchByIdAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var userId = new UserId(15);
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));

        // Act
        var result = await _repository.SearchByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchByIdAsync_FoundWithoutRecipes_ReturnsUser()
    {
        // Arrange
        var userId = new UserId(15);
        var username = Username.Create("Vovan").Value!;
        var password = new Password("$omeHa$hedPa$$word");
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync(
            "INSERT INTO Users (Id, Username, Password, Role) VALUES (@Id, @Username, @Password, @Role);", new
            {
                Id = userId.Value,
                Username = username.Value!,
                Password = password.PasswordHash,
                Role = UserRole.Classic.ToString()
            });

        // Act
        var result = await _repository.SearchByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(username, result.Username);
        Assert.Equal(password, result.Password);
        Assert.Equal(UserRole.Classic, result.Role);
    }

    [Fact]
    public async Task SearchByIdAsync_FoundWithRecipes_ReturnsUser()
    {
        #region Arrange

        var userId = new UserId(15);
        var username = Username.Create("Vovan").Value!;
        var password = new Password("$omeHa$hedPa$$word");
        var recipeId = new RecipeId(69);
        var title = RecipeTitle.Create("Some Amazing Title").Value;
        var imageName = new RecipeImageName("some-image.jpg");
        var difficulty = RecipeDifficulty.Hard;
        var cookingTime = TimeSpan.FromHours(4);
        var rating = new Rate(4.7m, 105);
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync(
            "INSERT INTO Users (Id, Username, Password, Role) VALUES (@Id, @Username, @Password, @Role);", new
            {
                Id = userId.Value,
                Username = username.Value!,
                Password = password.PasswordHash,
                Role = UserRole.Classic.ToString()
            });

        await db.ExecuteAsync(
            "INSERT INTO Recipes VALUES (@Id, @AuthorId, @Title, @Description, @Instruction, @ImageName, @Difficulty, @PublishedAt, @CookingTime, @Rating, @Votes);",
            new
            {
                Id = recipeId.Value,
                AuthorId = userId.Value,
                Title = title!.Value,
                Description = new string('b', 500),
                Instruction = new string('b', 500),
                ImageName = imageName.Value,
                Difficulty = difficulty.ToString(),
                PublishedAt = DateTimeOffset.UtcNow,
                CookingTime = cookingTime,
                Rating = rating.Value,
                Votes = rating.TotalVotes
            });

        #endregion

        #region Act

        var result = await _repository.SearchByIdAsync(userId);

        #endregion

        #region Assert

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(username, result.Username);
        Assert.Equal(password, result.Password);
        Assert.Equal(UserRole.Classic, result.Role);
        Assert.NotEmpty(result.Recipes);
        Assert.Equal(recipeId, result.Recipes.First().Id);

        #endregion
    }

    [Fact]
    public async Task UpdatePasswordAsync_ReturnsSuccess()
    {
        #region Arrange

        var userId = new UserId(6);
        var newPassword = "$0m3_=+=_@n0th3r_=+=_h@$hed_=+=_p@$$w0rd";
        _repository = new UserRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();
        await db.ExecuteAsync("INSERT INTO Users VALUES (@UserId, 'Vasyan', '$omeHa$hedPa$$w0rd', 'classic');",
            new { UserId = userId.Value });

        #endregion

        #region Act

        await _repository.UpdatePasswordAsync(new UserId(6), new Password("$0m3_=+=_@n0th3r_=+=_h@$hed_=+=_p@$$w0rd"));
        var result = await db.QuerySingleAsync<string>("SELECT Password FROM Users;");

        #endregion

        #region Assert

        Assert.Equal(newPassword, result);

        #endregion
    }
}