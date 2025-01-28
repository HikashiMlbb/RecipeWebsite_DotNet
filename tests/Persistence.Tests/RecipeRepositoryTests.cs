using Dapper;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Npgsql;
using Persistence.Repositories;
using Testcontainers.PostgreSql;
// ReSharper disable InconsistentNaming

namespace Persistence.Tests;

public class RecipeRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private RecipeRepository _repo;

    public RecipeRepositoryTests()
    {
        var builder = new PostgreSqlBuilder()
            .WithImage("postgres:latest");

        _container = builder.Build();
    }

    [Fact]
    public async Task InsertRecipe_WithoutIngredients_ReturnsRecipeId()
    {
        #region Arrange
        
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        var userId = db.QueryFirst<int>("INSERT INTO Users VALUES (DEFAULT, 'Vovan', 'Password', 'Classic') RETURNING Id;");
        var recipe = new Recipe(
            new UserId(userId),
            RecipeTitle.Create("SomeRecipeTitle").Value!,
            RecipeDescription.Create(new string('b', 5000)).Value!,
            RecipeInstruction.Create("Some interesting instruction").Value!,
            new RecipeImageName("Some.jpg"),
            RecipeDifficulty.Hard,
            TimeSpan.FromHours(6));

        #endregion

        #region Act

        var recipeId = await _repo.InsertAsync(recipe);
        var (dbId, dbAuthorId) = await db.QuerySingleAsync<(int, int)>("SELECT Id, Author_Id FROM Recipes;");

        #endregion

        #region Assert

        Assert.Equal(recipeId.Value, dbId);
        Assert.Equal(userId, dbAuthorId);

        #endregion
    }
    
    [Fact]
    public async Task InsertRecipe_IncludedIngredients_ReturnsRecipeId()
    {
        #region Arrange
        
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        var userId = db.QueryFirst<int>("INSERT INTO Users VALUES (16, 'Vovan', 'Password', 'Classic') RETURNING Id;");
        var ingredients = new[]
        {
            Ingredient.Create("Some One Ingredient Name", 5, IngredientType.Cups).Value!,
            Ingredient.Create("Some Two Ingredient Name", 200, IngredientType.Grams).Value!,
            Ingredient.Create("Some Three Ingredient Name", 3, IngredientType.Pieces).Value!
        };
        
        var recipe = new Recipe(
            new UserId(userId),
            RecipeTitle.Create("SomeRecipeTitle").Value!,
            RecipeDescription.Create(new string('b', 5000)).Value!,
            RecipeInstruction.Create("Some interesting instruction").Value!,
            new RecipeImageName("Some.jpg"),
            RecipeDifficulty.Hard,
            TimeSpan.FromHours(6),
            ingredients: ingredients);

        #endregion

        #region Act

        var recipeId = await _repo.InsertAsync(recipe);
        var (dbId, dbAuthorId) = await db.QuerySingleAsync<(int, int)>("SELECT Id, Author_Id FROM Recipes;");

        #endregion

        #region Assert

        Assert.Equal(recipeId.Value, dbId);
        Assert.Equal(userId, dbAuthorId);

        #endregion
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