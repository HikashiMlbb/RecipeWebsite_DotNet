using Dapper;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Persistence.Repositories;
using Testcontainers.PostgreSql;
// ReSharper disable InconsistentNaming

namespace Persistence.Tests;

public class RecipeRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private RecipeRepository _repo = null!;

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

    [Fact]
    public async Task SearchRecipeById_NotFound_ReturnsNull()
    {
        #region Arrange

        var recipeId = new RecipeId(144);
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();
        
        #endregion

        #region Act

        var recipe = await _repo.SearchByIdAsync(recipeId);

        #endregion

        #region Assert

        Assert.Null(recipe);

        #endregion
    }
    
    [Fact]
    public async Task SearchRecipeById_WithoutIngredients_WithoutComments_ReturnsRecipe()
    {
        #region Arrange

        var recipeId = new RecipeId(144);
        var userId = new UserId(69);
        var title = RecipeTitle.Create("Soup").Value!;
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("INSERT INTO Users VALUES (@Id, 'Vovan', 'A1234', 'classic')", new
        {
            @Id = userId.Value
        });
        
        await db.ExecuteAsync("INSERT INTO Recipes VALUES (@Id, @AuthorId, @Title, 'D', 'I', 'Img', 'hard', now(), '2h', 0, 0);", new
        {
            @Id = recipeId.Value,
            @AuthorId = userId.Value,
            @Title = title.Value
        });
        
        #endregion

        #region Act

        var recipe = await _repo.SearchByIdAsync(recipeId);

        #endregion

        #region Assert

        Assert.NotNull(recipe);
        Assert.Equal(recipeId, recipe.Id);
        Assert.Equal(title, recipe.Title);
        Assert.Empty(recipe.Ingredients);
        Assert.Empty(recipe.Comments);

        #endregion
    }
    
    [Fact]
    public async Task SearchRecipeById_IncludedIngredients_WithoutComments_ReturnsRecipeWithIngredients()
    {
        #region Arrange

        var recipeId = new RecipeId(144);
        var userId = new UserId(69);
        var title = RecipeTitle.Create("Soup").Value!;
        var ingredients = new[]
        {
            Ingredient.Create("Banana", 3, IngredientType.Pieces).Value!,
            Ingredient.Create("Coconut", 2, IngredientType.Pieces).Value!,
            Ingredient.Create("Milk", 4, IngredientType.Cups).Value!
        };
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("INSERT INTO Users VALUES (@Id, 'Vovan', 'A1234', 'classic')", new
        {
            @Id = userId.Value
        });
        
        await db.ExecuteAsync("INSERT INTO Recipes VALUES (@Id, @AuthorId, @Title, 'D', 'I', 'Img', 'hard', now(), '2h', 0, 0);", new
        {
            @Id = recipeId.Value,
            @AuthorId = userId.Value,
            @Title = title.Value
        });

        await db.ExecuteAsync(@"INSERT INTO Ingredients (Recipe_Id, Name, Count, Unit)
                VALUES (@RecipeId, @Name1, @Count1, @Unit1),
                (@RecipeId, @Name2, @Count2, @Unit2),
                (@RecipeId, @Name3, @Count3, @Unit3)", new
        {
            @RecipeId = recipeId.Value,
            @Name1 = ingredients[0].Name,
            @Count1 = ingredients[0].Count,
            @Unit1 = ingredients[0].UnitType.ToString(),
            @Name2 = ingredients[1].Name,
            @Count2 = ingredients[1].Count,
            @Unit2 = ingredients[1].UnitType.ToString(),
            @Name3 = ingredients[2].Name,
            @Count3 = ingredients[2].Count,
            @Unit3 = ingredients[2].UnitType.ToString(),
        });
        
        #endregion

        #region Act

        var recipe = await _repo.SearchByIdAsync(recipeId);

        #endregion

        #region Assert

        Assert.NotNull(recipe);
        Assert.Equal(recipeId, recipe.Id);
        Assert.Equal(title, recipe.Title);
        Assert.NotEmpty(recipe.Ingredients);
        Assert.Contains(ingredients[0], recipe.Ingredients);
        Assert.Contains(ingredients[1], recipe.Ingredients);
        Assert.Contains(ingredients[2], recipe.Ingredients);
        Assert.Empty(recipe.Comments);
        
        #endregion
    }
    
    [Fact]
    public async Task SearchRecipeById_IncludedIngredients_IncludedComments_ReturnsRecipeWithIngredientsAndComments()
    {
        #region Arrange

        var recipeId = new RecipeId(144);
        var userId = new UserId(69);
        var title = RecipeTitle.Create("Soup").Value!;
        var ingredients = new[]
        {
            Ingredient.Create("Banana", 3, IngredientType.Pieces).Value!,
            Ingredient.Create("Coconut", 2, IngredientType.Pieces).Value!,
            Ingredient.Create("Milk", 4, IngredientType.Cups).Value!
        };
        var comments = new[]
        {
            Comment.Create(new User { Id = userId, Username = Username.Create("Vovan").Value! }, "Very Interesting!").Value!,
            Comment.Create(new User { Id = new UserId(70), Username = Username.Create("Petr").Value! }, "I am first!", DateTimeOffset.Now - TimeSpan.FromDays(40)).Value!,
            Comment.Create(new User { Id = new UserId(71), Username = Username.Create("zxcursed").Value! }, "So delicious! I love it!", DateTimeOffset.UtcNow - TimeSpan.FromDays(14)).Value!
        };
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("INSERT INTO Users VALUES (@Id, 'Vovan', 'A1234', 'classic'), (70, 'Petr', 'SomePassword', 'classic'), (71, 'zxcursed', 'AnotherPassword', 'admin');", new
        {
            @Id = userId.Value
        });
        
        await db.ExecuteAsync("INSERT INTO Recipes VALUES (@Id, @AuthorId, @Title, 'D', 'I', 'Img', 'hard', now(), '2h', 0, 0);", new
        {
            @Id = recipeId.Value,
            @AuthorId = userId.Value,
            @Title = title.Value
        });

        await db.ExecuteAsync("""
                                  INSERT INTO Ingredients (Recipe_Id, Name, Count, Unit)
                                                  VALUES (@RecipeId, @Name1, @Count1, @Unit1),
                                                  (@RecipeId, @Name2, @Count2, @Unit2),
                                                  (@RecipeId, @Name3, @Count3, @Unit3)
                                  """, new
        {
            @RecipeId = recipeId.Value,
            @Name1 = ingredients[0].Name,
            @Count1 = ingredients[0].Count,
            @Unit1 = ingredients[0].UnitType.ToString(),
            @Name2 = ingredients[1].Name,
            @Count2 = ingredients[1].Count,
            @Unit2 = ingredients[1].UnitType.ToString(),
            @Name3 = ingredients[2].Name,
            @Count3 = ingredients[2].Count,
            @Unit3 = ingredients[2].UnitType.ToString(),
        });

        await db.ExecuteAsync(@"INSERT INTO Comments (Recipe_Id, User_Id, Content, Published_At)
                VALUES (@RecipeId, @UserId, @Content1, @PublishedAt1),
                       (@RecipeId, @UserId2, @Content2, @PublishedAt2),
                       (@RecipeId, @UserId3, @Content3, @PublishedAt3);", new
        {
            @RecipeId = recipeId.Value,
            @UserId = userId.Value,
            @UserId2 = 70,
            @UserId3 = 71,
            @Content1 = comments[0].Content,
            @PublishedAt1 = comments[0].PublishedAt.ToUniversalTime(),
            @Content2 = comments[1].Content,
            @PublishedAt2 = comments[1].PublishedAt.ToUniversalTime(),
            @Content3 = comments[2].Content,
            @PublishedAt3 = comments[2].PublishedAt.ToUniversalTime(),
        });
        #endregion

        #region Act

        var recipe = await _repo.SearchByIdAsync(recipeId);

        #endregion

        #region Assert

        Assert.NotNull(recipe);
        Assert.Equal(recipeId, recipe.Id);
        Assert.Equal(title, recipe.Title);
        Assert.NotEmpty(recipe.Ingredients);
        Assert.Contains(ingredients[0], recipe.Ingredients);
        Assert.Contains(ingredients[1], recipe.Ingredients);
        Assert.Contains(ingredients[2], recipe.Ingredients);
        Assert.NotEmpty(recipe.Comments);
        Assert.Equal(3, recipe.Comments.Count);
        
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