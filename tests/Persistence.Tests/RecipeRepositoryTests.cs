using Application.Recipes.GetByPage;
using Application.Recipes.Update;
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
    public async Task InsertRecipe_WithoutIngredients_ReturnsRecipeId()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        var userId =
            db.QueryFirst<int>("INSERT INTO \"Users\" VALUES (DEFAULT, 'Vovan', 'Password', 'Classic') RETURNING \"Id\";");
        var recipe = new Recipe(
            new User{ Id = new UserId(userId) },
            RecipeTitle.Create("SomeRecipeTitle").Value!,
            RecipeDescription.Create(new string('b', 5000)).Value!,
            RecipeInstruction.Create("Some interesting instruction").Value!,
            new RecipeImageName("Some.jpg"),
            RecipeDifficulty.Hard,
            TimeSpan.FromHours(6));

        #endregion

        #region Act

        var recipeId = await _repo.InsertAsync(recipe);
        var (dbId, dbAuthorId) = await db.QuerySingleAsync<(int, int)>("SELECT \"Id\", \"AuthorId\" FROM \"Recipes\";");

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

        var userId = db.QueryFirst<int>("INSERT INTO \"Users\" VALUES (16, 'Vovan', 'Password', 'Classic') RETURNING \"Id\";");
        var ingredients = new[]
        {
            Ingredient.Create("Some One Ingredient Name", 5, IngredientType.Cups).Value!,
            Ingredient.Create("Some Two Ingredient Name", 200, IngredientType.Grams).Value!,
            Ingredient.Create("Some Three Ingredient Name", 3, IngredientType.Pieces).Value!
        };

        var recipe = new Recipe(
            new User { Id = new UserId(userId) },
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
        var (dbId, dbAuthorId) = await db.QuerySingleAsync<(int, int)>("SELECT \"Id\", \"AuthorId\" FROM \"Recipes\";");

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

        await db.ExecuteAsync("INSERT INTO \"Users\" VALUES (@Id, 'Vovan', 'A1234', 'classic')", new
        {
            Id = userId.Value
        });

        await db.ExecuteAsync(
            "INSERT INTO \"Recipes\" VALUES (@Id, @AuthorId, @Title, 'D', 'I', 'Img', 'hard', now(), '2h', 0, 0);", new
            {
                Id = recipeId.Value,
                AuthorId = userId.Value,
                Title = title.Value
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

        await db.ExecuteAsync("INSERT INTO \"Users\" VALUES (@Id, 'Vovan', 'A1234', 'classic')", new
        {
            Id = userId.Value
        });

        await db.ExecuteAsync(
            "INSERT INTO \"Recipes\" VALUES (@Id, @AuthorId, @Title, 'D', 'I', 'Img', 'hard', now(), '2h', 0, 0);", new
            {
                Id = recipeId.Value,
                AuthorId = userId.Value,
                Title = title.Value
            });

        await db.ExecuteAsync("""
                              INSERT INTO "Ingredients" ("RecipeId", "Name", "Count", "Unit") VALUES 
                              (@RecipeId, @Name1, @Count1, @Unit1),
                              (@RecipeId, @Name2, @Count2, @Unit2),
                              (@RecipeId, @Name3, @Count3, @Unit3)
                              """, new
        {
            RecipeId = recipeId.Value,
            Name1 = ingredients[0].Name,
            Count1 = ingredients[0].Count,
            Unit1 = ingredients[0].UnitType.ToString(),
            Name2 = ingredients[1].Name,
            Count2 = ingredients[1].Count,
            Unit2 = ingredients[1].UnitType.ToString(),
            Name3 = ingredients[2].Name,
            Count3 = ingredients[2].Count,
            Unit3 = ingredients[2].UnitType.ToString()
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
            new Comment(new User { Id = userId, Username = Username.Create("Vovan").Value! }, "Very Interesting!",
                DateTimeOffset.UtcNow),
            new Comment(new User { Id = new UserId(70), Username = Username.Create("Petr").Value! }, "I am first!",
                DateTimeOffset.Now - TimeSpan.FromDays(40)),
            new Comment(new User { Id = new UserId(71), Username = Username.Create("zxcursed").Value! },
                "So delicious! I love it!", DateTimeOffset.UtcNow - TimeSpan.FromDays(14))
        };
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync(
            "INSERT INTO \"Users\" VALUES (@Id, 'Vovan', 'A1234', 'classic'), (70, 'Petr', 'SomePassword', 'classic'), (71, 'zxcursed', 'AnotherPassword', 'admin');",
            new
            {
                Id = userId.Value
            });

        await db.ExecuteAsync(
            "INSERT INTO \"Recipes\" VALUES (@Id, @AuthorId, @Title, 'D', 'I', 'Img', 'hard', now(), '2h', 0, 0);", new
            {
                Id = recipeId.Value,
                AuthorId = userId.Value,
                Title = title.Value
            });

        await db.ExecuteAsync("""
                              INSERT INTO "Ingredients" ("RecipeId", "Name", "Count", "Unit") VALUES
                              (@RecipeId, @Name1, @Count1, @Unit1),
                              (@RecipeId, @Name2, @Count2, @Unit2),
                              (@RecipeId, @Name3, @Count3, @Unit3)
                              """, new
        {
            RecipeId = recipeId.Value,
            Name1 = ingredients[0].Name,
            Count1 = ingredients[0].Count,
            Unit1 = ingredients[0].UnitType.ToString(),
            Name2 = ingredients[1].Name,
            Count2 = ingredients[1].Count,
            Unit2 = ingredients[1].UnitType.ToString(),
            Name3 = ingredients[2].Name,
            Count3 = ingredients[2].Count,
            Unit3 = ingredients[2].UnitType.ToString()
        });

        await db.ExecuteAsync("""
                              INSERT INTO "Comments" ("RecipeId", "UserId", "Content", "PublishedAt") VALUES
                              (@RecipeId, @UserId, @Content1, @PublishedAt1),
                              (@RecipeId, @UserId2, @Content2, @PublishedAt2),
                              (@RecipeId, @UserId3, @Content3, @PublishedAt3);
                              """, new
        {
            RecipeId = recipeId.Value,
            UserId = userId.Value,
            UserId2 = 70,
            UserId3 = 71,
            Content1 = comments[0].Content,
            PublishedAt1 = comments[0].PublishedAt.ToUniversalTime(),
            Content2 = comments[1].Content,
            PublishedAt2 = comments[1].PublishedAt.ToUniversalTime(),
            Content3 = comments[2].Content,
            PublishedAt3 = comments[2].PublishedAt.ToUniversalTime()
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

    [Fact]
    public async Task RateAsync_SetRate()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO "Users" ("Id", "Username", "Password", "Role")
                              VALUES (123, 'Peter', 'Password1234', 'classic');

                              INSERT INTO "Recipes" ("Id", "AuthorId", "Title", "Description", "Instruction", "ImageName", "Difficulty", "PublishedAt", "CookingTime", "Rating", "Votes")
                              VALUES (456, 123, 'RecipeTitle', 'RecipeD', 'RI', 'IMG', 'hard', now(), '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var result = await _repo.RateAsync(new RecipeId(456), new UserId(123), Stars.Five);
        var (rate, votes) =
            await db.QueryFirstAsync<(decimal, int)>("SELECT \"Rating\", \"Votes\" FROM \"Recipes\" WHERE \"Id\" = 456");

        #endregion

        #region Assert

        Assert.Equal(Stars.Five, result);
        Assert.Equal(5.0m, rate);
        Assert.Equal(1, votes);

        #endregion
    }

    [Fact]
    public async Task RateAsync_SetRateTwice()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO "Users" ("Id", "Username", "Password", "Role")
                              VALUES (123, 'Peter', 'Password1234', 'classic'),
                                     (321, 'Ivan', '1234Password', 'classic');

                              INSERT INTO "Recipes" ("Id", "AuthorId", "Title", "Description", "Instruction", "ImageName", "Difficulty", "PublishedAt", "CookingTime", "Rating", "Votes")
                              VALUES (456, 123, 'RecipeTitle', 'RecipeD', 'RI', 'IMG', 'hard', now(), '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var rate1 = await _repo.RateAsync(new RecipeId(456), new UserId(123), Stars.Five);
        var rate2 = await _repo.RateAsync(new RecipeId(456), new UserId(321), Stars.Three);
        var (rate, votes) =
            await db.QueryFirstAsync<(decimal, int)>("SELECT \"Rating\", \"Votes\" FROM \"Recipes\" WHERE \"Id\" = 456");

        #endregion

        #region Assert

        Assert.Equal(Stars.Five, rate1);
        Assert.Equal(Stars.Three, rate2);
        Assert.Equal(4.0m, rate);
        Assert.Equal(2, votes);

        #endregion
    }

    [Fact]
    public async Task RateAsync_ChangeRate()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO "Users" ("Id", "Username", "Password", "Role")
                              VALUES (123, 'Peter', 'Password1234', 'classic');

                              INSERT INTO "Recipes" ("Id", "AuthorId", "Title", "Description", "Instruction", "ImageName", "Difficulty", "PublishedAt", "CookingTime", "Rating", "Votes")
                              VALUES (456, 123, 'RecipeTitle', 'RecipeD', 'RI', 'IMG', 'hard', now(), '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var rate1 = await _repo.RateAsync(new RecipeId(456), new UserId(123), Stars.Five);
        var rate2 = await _repo.RateAsync(new RecipeId(456), new UserId(123), Stars.Three);
        var (rate, votes) =
            await db.QueryFirstAsync<(decimal, int)>("SELECT \"Rating\", \"Votes\" FROM \"Recipes\" WHERE \"Id\" = 456");

        #endregion

        #region Assert

        Assert.Equal(Stars.Five, rate1);
        Assert.Equal(Stars.Three, rate2);
        Assert.Equal(3.0m, rate);
        Assert.Equal(1, votes);

        #endregion
    }

    [Fact]
    public async Task RateAsync_UndoRate()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO "Users" ("Id", "Username", "Password", "Role")
                              VALUES (123, 'Peter', 'Password1234', 'classic');

                              INSERT INTO "Recipes" ("Id", "AuthorId", "Title", "Description", "Instruction", "ImageName", "Difficulty", "PublishedAt", "CookingTime", "Rating", "Votes")
                              VALUES (456, 123, 'RecipeTitle', 'RecipeD', 'RI', 'IMG', 'hard', now(), '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var rate1 = await _repo.RateAsync(new RecipeId(456), new UserId(123), Stars.Five);
        var rate2 = await _repo.RateAsync(new RecipeId(456), new UserId(123), Stars.Five);
        var (rate, votes) =
            await db.QueryFirstAsync<(decimal, int)>("SELECT \"Rating\", \"Votes\" FROM \"Recipes\" WHERE \"Id\" = 456");

        #endregion

        #region Assert

        Assert.Equal(Stars.Five, rate1);
        Assert.Equal(Stars.Zero, rate2);
        Assert.Equal(0, rate);
        Assert.Equal(0, votes);

        #endregion
    }

    [Fact]
    public async Task Comment_Successfully()
    {
        #region Arrange

        var recipeId = new RecipeId(14);
        var userId = new UserId(69);
        var user = new User { Id = userId };
        var comment = new Comment(user, "Very good!", DateTimeOffset.Now.ToUniversalTime());
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO "Users" ("Id", "Username", "Password", "Role")
                              VALUES (@UserId, 'Pavel', 'SomePassword', 'classic');

                              INSERT INTO "Recipes" ("Id", "AuthorId", "Title", "Description", "Instruction", "ImageName", "Difficulty", "PublishedAt", "CookingTime", "Rating", "Votes")
                              VALUES (@RecipeId, @UserId, 'T', 'D', 'I', 'IMG', 'hard', now(), '2h', 0, 0);
                              """, new
        {
            UserId = userId.Value,
            RecipeId = recipeId.Value
        });

        #endregion

        #region Act

        await _repo.CommentAsync(recipeId, comment);
        var (commentRecipeId, commentAuthorId, content, publicationDate) =
            await db.QueryFirstAsync<(int, int, string, DateTimeOffset)>(
                "SELECT \"RecipeId\", \"UserId\", \"Content\", \"PublishedAt\" FROM \"Comments\";");

        #endregion

        #region Assert

        Assert.Equal(comment.Author.Id.Value, commentAuthorId);
        Assert.Equal(recipeId.Value, commentRecipeId);
        Assert.Equal(comment.Content, content);
        Assert.Equal(comment.PublishedAt, publicationDate, TimeSpan.FromMilliseconds(1));

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_RecipesEmpty()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));

        #endregion

        #region Act

        var result = await _repo.SearchByPageAsync(1, 10, RecipeSortType.Popular);

        #endregion

        #region Assert

        Assert.Empty(result);

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_FullOnePage_PopularFirst()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic'),
                                     (102, 'Username2', 'Password', 'classic'),
                                     (103, 'Username3', 'Password', 'classic'),
                                     (104, 'Username4', 'Password', 'classic'),
                                     (105, 'Username5', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 102, 'T', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 103, 'T', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 104, 'T', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 105, 'T', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);

                              INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                              VALUES (503, 101, 1),
                                     (503, 102, 1),
                                     (503, 103, 1),
                                     (503, 104, 1),
                                     (503, 105, 1),
                                     (502, 101, 5),
                                     (502, 103, 5),
                                     (502, 104, 5),
                                     (501, 102, 4),
                                     (501, 103, 4),
                                     (501, 104, 4);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByPageAsync(1, 5, RecipeSortType.Popular)).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Equal(5, result.Count);
        Assert.Equal(new RecipeId(503), result[0].Id);
        Assert.Equal(new RecipeId(502), result[1].Id);
        Assert.Equal(new RecipeId(501), result[2].Id);

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_FullOnePage_NewestFirst()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic'),
                                     (102, 'Username2', 'Password', 'classic'),
                                     (103, 'Username3', 'Password', 'classic'),
                                     (104, 'Username4', 'Password', 'classic'),
                                     (105, 'Username5', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 102, 'T', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 103, 'T', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 104, 'T', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 105, 'T', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);

                              INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                              VALUES (503, 101, 1),
                                     (503, 102, 1),
                                     (503, 103, 1),
                                     (503, 104, 1),
                                     (503, 105, 1),
                                     (502, 101, 5),
                                     (502, 103, 5),
                                     (502, 104, 5),
                                     (501, 102, 4),
                                     (501, 103, 4),
                                     (501, 104, 4);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByPageAsync(1, 5, RecipeSortType.Newest)).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Equal(5, result.Count);
        Assert.Equal(new RecipeId(505), result[0].Id);
        Assert.Equal(new RecipeId(504), result[1].Id);
        Assert.Equal(new RecipeId(501), result[2].Id);
        Assert.Equal(new RecipeId(502), result[3].Id);
        Assert.Equal(new RecipeId(503), result[4].Id);

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_SlicedFirstPage_PopularFirst()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic'),
                                     (102, 'Username2', 'Password', 'classic'),
                                     (103, 'Username3', 'Password', 'classic'),
                                     (104, 'Username4', 'Password', 'classic'),
                                     (105, 'Username5', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 102, 'T', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 103, 'T', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 104, 'T', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 105, 'T', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);

                              INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                              VALUES (503, 101, 1),
                                     (503, 102, 1),
                                     (503, 103, 1),
                                     (503, 104, 1),
                                     (503, 105, 1),
                                     (502, 101, 5),
                                     (502, 103, 5),
                                     (502, 104, 5),
                                     (501, 102, 4),
                                     (501, 103, 4),
                                     (501, 104, 4);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByPageAsync(1, 2, RecipeSortType.Popular)).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(new RecipeId(503), result[0].Id);
        Assert.Equal(new RecipeId(502), result[1].Id);

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_SlicedFirstPage_NewestFirst()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic'),
                                     (102, 'Username2', 'Password', 'classic'),
                                     (103, 'Username3', 'Password', 'classic'),
                                     (104, 'Username4', 'Password', 'classic'),
                                     (105, 'Username5', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 102, 'T', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 103, 'T', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 104, 'T', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 105, 'T', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);

                              INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                              VALUES (503, 101, 1),
                                     (503, 102, 1),
                                     (503, 103, 1),
                                     (503, 104, 1),
                                     (503, 105, 1),
                                     (502, 101, 5),
                                     (502, 103, 5),
                                     (502, 104, 5),
                                     (501, 102, 4),
                                     (501, 103, 4),
                                     (501, 104, 4);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByPageAsync(1, 2, RecipeSortType.Newest)).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(new RecipeId(505), result[0].Id);
        Assert.Equal(new RecipeId(504), result[1].Id);

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_SlicedSecondPage_PopularFirst()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic'),
                                     (102, 'Username2', 'Password', 'classic'),
                                     (103, 'Username3', 'Password', 'classic'),
                                     (104, 'Username4', 'Password', 'classic'),
                                     (105, 'Username5', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 102, 'T', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 103, 'T', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 104, 'T', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 105, 'T', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);

                              INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                              VALUES (503, 101, 1),
                                     (503, 102, 1),
                                     (503, 103, 1),
                                     (503, 104, 1),
                                     (503, 105, 1),
                                     (502, 101, 5),
                                     (502, 103, 5),
                                     (502, 104, 5),
                                     (501, 102, 4),
                                     (501, 103, 4),
                                     (501, 104, 4);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByPageAsync(2, 2, RecipeSortType.Popular)).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(new RecipeId(501), result[0].Id);

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_SlicedSecondPage_NewestFirst()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic'),
                                     (102, 'Username2', 'Password', 'classic'),
                                     (103, 'Username3', 'Password', 'classic'),
                                     (104, 'Username4', 'Password', 'classic'),
                                     (105, 'Username5', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 102, 'T', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 103, 'T', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 104, 'T', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 105, 'T', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);

                              INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                              VALUES (503, 101, 1),
                                     (503, 102, 1),
                                     (503, 103, 1),
                                     (503, 104, 1),
                                     (503, 105, 1),
                                     (502, 101, 5),
                                     (502, 103, 5),
                                     (502, 104, 5),
                                     (501, 102, 4),
                                     (501, 103, 4),
                                     (501, 104, 4);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByPageAsync(2, 2, RecipeSortType.Newest)).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(new RecipeId(501), result[0].Id);
        Assert.Equal(new RecipeId(502), result[1].Id);

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_SlicedThirdPage_PopularFirst()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic'),
                                     (102, 'Username2', 'Password', 'classic'),
                                     (103, 'Username3', 'Password', 'classic'),
                                     (104, 'Username4', 'Password', 'classic'),
                                     (105, 'Username5', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 102, 'T', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 103, 'T', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 104, 'T', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 105, 'T', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);

                              INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                              VALUES (503, 101, 1),
                                     (503, 102, 1),
                                     (503, 103, 1),
                                     (503, 104, 1),
                                     (503, 105, 1),
                                     (502, 101, 5),
                                     (502, 103, 5),
                                     (502, 104, 5),
                                     (501, 102, 4),
                                     (501, 103, 4),
                                     (501, 104, 4),
                                     (505, 104, 4);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByPageAsync(3, 2, RecipeSortType.Popular)).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Equal(new RecipeId(504), result[0].Id);

        #endregion
    }

    [Fact]
    public async Task SearchByPageAsync_SlicedThirdPage_NewestFirst()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic'),
                                     (102, 'Username2', 'Password', 'classic'),
                                     (103, 'Username3', 'Password', 'classic'),
                                     (104, 'Username4', 'Password', 'classic'),
                                     (105, 'Username5', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 102, 'T', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 103, 'T', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 104, 'T', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 105, 'T', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);

                              INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                              VALUES (503, 101, 1),
                                     (503, 102, 1),
                                     (503, 103, 1),
                                     (503, 104, 1),
                                     (503, 105, 1),
                                     (502, 101, 5),
                                     (502, 103, 5),
                                     (502, 104, 5),
                                     (501, 102, 4),
                                     (501, 103, 4),
                                     (501, 104, 4);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByPageAsync(3, 2, RecipeSortType.Newest)).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Equal(new RecipeId(503), result[0].Id);

        #endregion
    }

    [Fact]
    public async Task SearchByQueryAsync_ReturnsNull()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));

        #endregion

        #region Act

        var result = (await _repo.SearchByQueryAsync("soup")).ToList();

        #endregion

        #region Assert

        Assert.Empty(result);

        #endregion
    }

    [Fact]
    public async Task SearchByQueryAsync_WithRecipes_NotFound()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'T1', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 101, 'T2', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 101, 'T3', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 101, 'T4', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 101, 'T5', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByQueryAsync("soup")).ToList();

        #endregion

        #region Assert

        Assert.Empty(result);

        #endregion
    }

    [Fact]
    public async Task SearchByQueryAsync_WithRecipes_Found_OnlyTitle()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'Brazilian Soup', 'D', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 101, 'Australian Soup', 'D', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 101, 'Russian Soup', 'D', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 101, 'Indian Soup', 'D', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 101, 'Bolon''eze', 'D', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByQueryAsync("soup")).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Equal(4, result.Count);

        #endregion
    }

    [Fact]
    public async Task SearchByQueryAsync_WithRecipes_Found_OnlyDescription()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'Brazilian Soup', 'Some recipe for soup from Brazil', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 101, 'Australian Soup', 'Some recipe for soup from Australia', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 101, 'Russian Soup', 'Some recipe for soup from Russia', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 101, 'Indian Soup', 'Some recipe for soup from India', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 101, 'Bolon''eze', 'Some recipe for bolon''eze from Italy', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByQueryAsync("italy")).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Single(result);

        #endregion
    }

    [Fact]
    public async Task SearchByQueryAsync_WithRecipes_Found_BothTitleAndDescription()
    {
        #region Arrange

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (101, 'Username1', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (501, 101, 'Brazilian Soup key', 'Some recipe for soup from Brazil key', 'I', 'IMG', 'easy', '2023-05-01 13:13:15+00', '2h', 0, 0),
                                     (502, 101, 'Australian Soup key', 'Some recipe for soup from Australia', 'I', 'IMG', 'easy', '2022-05-01 13:13:15+00', '2h', 0, 0),
                                     (503, 101, 'Russian Soup', 'Some recipe for soup from Russia key', 'I', 'IMG', 'easy', '2021-05-01 13:13:15+00', '2h', 0, 0),
                                     (504, 101, 'Indian Soup', 'Some recipe for soup from India', 'I', 'IMG', 'easy', '2024-05-01 13:13:15+00', '2h', 0, 0),
                                     (505, 101, 'Bolon''eze', 'Some recipe for bolon''eze from Italy', 'I', 'IMG', 'easy', '2025-01-05 13:13:15+00', '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var result = (await _repo.SearchByQueryAsync("key")).ToList();

        #endregion

        #region Assert

        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);

        #endregion
    }

    [Fact]
    public async Task Update_NothingToUpdate()
    {
        #region Arrage

        var updateConfig = new RecipeUpdateConfig(new RecipeId(15));
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (4, 'Username', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (15, 4, 'Title', 'Description', 'Instruction', 'Image Name', 'hard', now(), '2h', 0, 0);
                              """);

        #endregion

        #region Act

        await _repo.UpdateAsync(updateConfig);
        var title = await db.QueryFirstAsync<string>("SELECT Title FROM Recipes WHERE Id = 15");

        #endregion

        #region Assert

        Assert.Equal("Title", title);

        #endregion
    }

    [Fact]
    public async Task Update_FullUpdate_ExcludedIngredients()
    {
        #region Arrage

        var updateConfig = new RecipeUpdateConfig(
            new RecipeId(15),
            new RecipeTitle("Some New Title"),
            new RecipeDescription("Some New Description"),
            new RecipeInstruction("Some New Instruction"),
            new RecipeImageName("Some New Image Name"),
            RecipeDifficulty.Medium,
            TimeSpan.FromHours(1));
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (4, 'Username', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (15, 4, 'Title', 'Description', 'Instruction', 'Image Name', 'hard', now(), '2h', 0, 0);
                              """);

        #endregion

        #region Act

        await _repo.UpdateAsync(updateConfig);
        var (title, description, instruction, imageName, difficulty, cookingTime) =
            await db.QueryFirstAsync<(string, string, string, string, string, TimeSpan)>(
                "SELECT Title, Description, Instruction, Image_Name, Difficulty, Cooking_Time FROM Recipes WHERE Id = 15");

        #endregion

        #region Assert

        Assert.Equal(updateConfig.Title!.Value, title);
        Assert.Equal(updateConfig.Description!.Value, description);
        Assert.Equal(updateConfig.Instruction!.Value, instruction);
        Assert.Equal(updateConfig.ImageName!.Value, imageName);
        Assert.Equal(updateConfig.Difficulty!.ToString(), difficulty);
        Assert.Equal(updateConfig.CookingTime, cookingTime);

        #endregion
    }

    [Fact]
    public async Task Update_SomeIngredients_To_EmptyIngredients()
    {
        #region Arrage

        var updateConfig = new RecipeUpdateConfig(new RecipeId(15), Ingredients: []);
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (4, 'Username', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (15, 4, 'Title', 'Description', 'Instruction', 'Image Name', 'hard', now(), '2h', 0, 0);

                              INSERT INTO Ingredients (Id, Recipe_Id, Name, Count, Unit)
                              VALUES (DEFAULT, 15, 'Coconut', 5, 'pieces'),
                                     (DEFAULT, 15, 'Milk', 300, 'milliliters'),
                                     (DEFAULT, 15, 'Sugar', 15, 'grams');
                              """);

        #endregion

        #region Act

        await _repo.UpdateAsync(updateConfig);
        var ingredientCount = await db.QueryFirstAsync<int>("SELECT COUNT(*) FROM Ingredients WHERE Recipe_Id = 15;");

        #endregion

        #region Assert

        Assert.Equal(updateConfig.Ingredients!.Count, ingredientCount);

        #endregion
    }

    [Fact]
    public async Task Update_SomeIngredients_To_SomeIngredients()
    {
        #region Arrage

        var updateConfig = new RecipeUpdateConfig(
            new RecipeId(15),
            Ingredients:
            [
                new Ingredient("Coconut", 5, IngredientType.Pieces),
                new Ingredient("Milk", 5, IngredientType.Pieces),
                new Ingredient("Sugar", 5, IngredientType.Pieces)
            ]);
        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (4, 'Username', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (15, 4, 'Title', 'Description', 'Instruction', 'Image Name', 'hard', now(), '2h', 0, 0);

                              INSERT INTO Ingredients (Id, Recipe_Id, Name, Count, Unit)
                              VALUES (DEFAULT, 15, 'Water', 1500, 'milliliters'),
                                     (DEFAULT, 15, 'Salt', 15, 'grams');
                              """);

        #endregion

        #region Act

        await _repo.UpdateAsync(updateConfig);
        var ingredientCount = await db.QueryFirstAsync<int>("SELECT COUNT(*) FROM Ingredients WHERE Recipe_Id = 15;");

        #endregion

        #region Assert

        Assert.Equal(updateConfig.Ingredients!.Count, ingredientCount);

        #endregion
    }

    [Fact]
    public async Task Delete_Success()
    {
        #region Arrage

        _repo = new RecipeRepository(new DapperConnectionFactory(_container.GetConnectionString()));
        await using var db = new DapperConnectionFactory(_container.GetConnectionString()).Create();
        await db.OpenAsync();

        await db.ExecuteAsync("""
                              INSERT INTO Users (Id, Username, Password, Role)
                              VALUES (4, 'Username', 'Password', 'classic');

                              INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                              VALUES (15, 4, 'Title', 'Description', 'Instruction', 'Image Name', 'hard', now(), '2h', 0, 0);
                              """);

        #endregion

        #region Act

        var userRecipesCountBefore = await db.QueryFirstAsync<int>("SELECT COUNT(*) FROM Recipes WHERE Author_Id = 4;");
        await _repo.DeleteAsync(new RecipeId(15));
        var userRecipesCountAfter = await db.QueryFirstAsync<int>("SELECT COUNT(*) FROM Recipes WHERE Author_Id = 4;");

        #endregion

        #region Assert

        Assert.Equal(1, userRecipesCountBefore);
        Assert.Equal(0, userRecipesCountAfter);

        #endregion
    }
}