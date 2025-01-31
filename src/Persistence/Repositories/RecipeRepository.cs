using System.Text;
using Application.Recipes;
using Application.Recipes.GetByPage;
using Application.Recipes.Update;
using Dapper;
using Dapper.Transaction;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Persistence.Repositories.Dto;

namespace Persistence.Repositories;

public class RecipeRepository(DapperConnectionFactory factory) : IRecipeRepository
{
    public async Task<RecipeId> InsertAsync(Recipe newRecipe)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        int recipeId;
        
        await using var transaction = await db.BeginTransactionAsync();

        const string recipeSql = """
                                 INSERT INTO Recipes (Id, Author_Id, Title, Description, Instruction, Image_Name, Difficulty, Published_At, Cooking_Time, Rating, Votes)
                                 VALUES (DEFAULT, @UserId, @Title, @Description, @Instruction, @ImageName, @Difficulty, @PublishedAt, @CookingTime, @Rating, @Votes)
                                 RETURNING Id;
                                 """;

        const string ingredientSql = """
                                     INSERT INTO Ingredients (Recipe_Id, Name, Count, Unit)
                                     VALUES (@RecipeId, @Name, @Count, @Unit);
                                     """;
        
        try
        {
            recipeId = await transaction.QueryFirstAsync<int>(recipeSql, new
            {
                @UserId = newRecipe.AuthorId.Value,
                @Title = newRecipe.Title.Value,
                @Description = newRecipe.Description.Value,
                @Instruction = newRecipe.Instruction.Value,
                @ImageName = newRecipe.ImageName.Value,
                @Difficulty = newRecipe.Difficulty.ToString(),
                @PublishedAt = newRecipe.PublishedAt.ToUniversalTime(),
                @CookingTime = newRecipe.CookingTime,
                @Rating = 0,
                @Votes = 0
            });

            foreach (var ingredient in newRecipe.Ingredients)
            {
                await transaction.ExecuteAsync(ingredientSql, new
                {
                    @RecipeId = recipeId,
                    @Name = ingredient.Name,
                    @Count = ingredient.Count,
                    @Unit = ingredient.UnitType.ToString()
                });
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return new RecipeId(recipeId);
    }

    public async Task<Recipe?> SearchByIdAsync(RecipeId recipeId)
    {
        await using var db = factory.Create();
        await db.OpenAsync();
        
        const string sql = """
                           SELECT 
                                recipes.Id AS RecipeId,
                                recipes.Author_Id AS AuthorId,
                                recipes.Title,
                                recipes.Description,
                                recipes.Instruction,
                                recipes.Image_Name AS ImageName,
                                recipes.Difficulty,
                                recipes.Published_At AS PublishedAt,
                                recipes.Cooking_Time AS CookingTime,
                                recipes.Rating,
                                recipes.Votes,
                                ingredients.Id AS IngredientId,
                                ingredients.Name,
                                ingredients.Count,
                                ingredients.Unit AS UnitType,
                                comments.Id AS CommentId,
                                comments.Recipe_Id AS CommentSplit,
                                comments.Content AS Content,
                                comments.Published_At AS CommentPublishedAt,
                                users.Id AS UserId,
                                users.Username
                           FROM Recipes recipes
                           LEFT OUTER JOIN Ingredients ingredients ON ingredients.Recipe_Id = recipes.Id
                           LEFT OUTER JOIN Comments comments ON comments.Recipe_Id = recipes.Id
                           LEFT OUTER JOIN Users users ON users.Id = comments.User_Id
                           WHERE recipes.Id = @Id;
                           """;

        RecipeDatabaseDto? detailedDto = null;

        var result =
            (await db
                .QueryAsync<RecipeDatabaseDto, IngredientDatabaseDto, CommentDatabaseDto, UserDatabaseDto,
                    RecipeDatabaseDto>(
                    sql,
                    (recipeDto, ingredientDto, commentDto, userDto) =>
                    {
                        detailedDto ??= recipeDto;
                        if (ingredientDto?.Count > 0 && detailedDto.Ingredients.SingleOrDefault(x => x.IngredientId == ingredientDto.IngredientId) is null)
                            detailedDto.Ingredients.Add(ingredientDto);

                        if (commentDto?.Content is not null && detailedDto.Comments.SingleOrDefault(x => x.CommentId == commentDto.CommentId) is null)
                            detailedDto.Comments.Add(commentDto with { Author = userDto });
                        
                        return detailedDto;
                    },
                    splitOn: "IngredientId, CommentId, UserId",
                    param: new
                    {
                        Id = recipeId.Value
                    })).ToList();
        
        if (result.Count == 0)
        {
            return null;
        }

        var uniqueResult = result.Distinct().Single();
        

        var ingredients = uniqueResult.Ingredients.Count == 0
            ? Array.Empty<Ingredient>()
            : uniqueResult.Ingredients.Select(x => Ingredient.Create(x.Name, x.Count, Enum.Parse<IngredientType>(x.UnitType, ignoreCase: true)).Value).AsList() as ICollection<Ingredient>;

        var comments = uniqueResult.Comments.Count == 0
            ? Array.Empty<Comment>()
            : uniqueResult.Comments.Select(x => Comment.Create(new User { Id = new UserId(x.Author.UserId), Username = Username.Create(x.Author.Username).Value! }, x.Content, x.CommentPublishedAt).Value!).AsList() as ICollection<Comment>;
        
        return new Recipe
        {
            Id = new RecipeId(uniqueResult.RecipeId),
            AuthorId = new UserId(uniqueResult.AuthorId),
            Title = RecipeTitle.Create(uniqueResult.Title).Value!,
            Description = RecipeDescription.Create(uniqueResult.Description).Value!,
            Instruction = RecipeInstruction.Create(uniqueResult.Instruction).Value!,
            ImageName = new RecipeImageName(uniqueResult.ImageName),
            Difficulty = Enum.Parse<RecipeDifficulty>(uniqueResult.Difficulty, ignoreCase: true),
            PublishedAt = uniqueResult.PublishedAt,
            CookingTime = uniqueResult.CookingTime,
            Rate = new Rate(uniqueResult.Rating, uniqueResult.Votes),
            Ingredients = ingredients,
            Comments = comments
        };
    }

    public async Task<Stars> RateAsync(RecipeId recipeId, UserId userId, Stars rate)
    {
        await using var db = factory.Create();
        await db.OpenAsync();
        
        const string sql = "SELECT Rate_Recipe(@RecipeId, @UserId, @Rate)";
        
        var newRate = await db.QueryFirstAsync<int>(sql, new
        {
            @RecipeId = recipeId.Value,
            @UserId = userId.Value,
            @Rate = (int)rate
        });

        return (Stars)newRate;
    }

    public async Task CommentAsync(RecipeId recipeId, Comment comment)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        const string sql = """
                           INSERT INTO Comments (Id, Recipe_Id, User_Id, Content, Published_At)
                           VALUES (DEFAULT, @RecipeId, @UserId, @Content, @PublishedAt);
                           """;
        await db.ExecuteAsync(sql, new
        {
            @RecipeId = recipeId.Value,
            @UserId = comment.Author.Id.Value,
            @Content = comment.Content,
            @PublishedAt = comment.PublishedAt
        });
    }

    public async Task<IEnumerable<Recipe>> SearchByPageAsync(int page, int pageSize, RecipeSortType sortType)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        const string sqlBeginning = """
                           SELECT
                                Id AS RecipeId,
                                Title,
                                Image_Name AS ImageName,
                                Difficulty,
                                Cooking_Time AS CookingTime,
                                Rating,
                                Votes
                           FROM Recipes
                           
                           """;
        
        const string sqlEnding = """
                                 
                                 LIMIT @Limit
                                 OFFSET @Offset;
                                 """;
        
        const string popular = """
                               ORDER BY
                                    Votes DESC,
                                    Rating DESC
                               """;
        
        const string newest = "ORDER BY Published_At DESC";

        var chosenSort = sortType switch
        {
            RecipeSortType.Popular => popular,
            RecipeSortType.Newest => newest,
            _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null)
        };

        var sql = sqlBeginning + chosenSort + sqlEnding;

        var foundRawRecipes = await db.QueryAsync<RecipeDatabaseDto>(sql, new
        {
            @Limit = pageSize,
            @Offset = (page - 1) * pageSize
        });

        return foundRawRecipes.Select(x => new Recipe
        {
            Id = new RecipeId(x.RecipeId),
            Title = new RecipeTitle(x.Title),
            ImageName = new RecipeImageName(x.ImageName),
            Difficulty = Enum.Parse<RecipeDifficulty>(x.Difficulty, ignoreCase: true),
            CookingTime = x.CookingTime,
            Rate = new Rate(x.Rating, x.Votes)
        }).ToList();
    }

    public async Task<IEnumerable<Recipe>> SearchByQueryAsync(string query)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        const string sql = """
                            SELECT
                                 Id AS RecipeId,
                                 Title,
                                 Image_Name AS ImageName,
                                 Difficulty,
                                 Cooking_Time AS CookingTime,
                                 Rating,
                                 Votes
                            FROM Recipes
                            WHERE LOWER(Title) LIKE LOWER(@Query) OR LOWER(Description) LIKE LOWER(@Query)
                            ORDER BY 
                                CASE
                                    WHEN LOWER(Title) LIKE LOWER(@Query) THEN 1
                                    ELSE 2
                                END;
                            """;

        var result = await db.QueryAsync<RecipeDatabaseDto>(sql, new
        {
            @Query = $"%{query}%"
        });

        return result.Select(x => new Recipe
        {
            Id = new RecipeId(x.RecipeId),
            Title = new RecipeTitle(x.Title),
            ImageName = new RecipeImageName(x.ImageName),
            Difficulty = Enum.Parse<RecipeDifficulty>(x.Difficulty, ignoreCase: true),
            Rate = new Rate(x.Rating, x.Votes)
        });
    }

    public async Task UpdateAsync(RecipeUpdateConfig updateConfig)
    {
        await using var db = factory.Create();
        await db.OpenAsync();
        
        const string sqlBeginning = """
                                    UPDATE Recipes
                                    SET
                                    """;
        const string sqlEnding = "WHERE Id = @RecipeId;";

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append(sqlBeginning);
        sqlBuilder.Append("\n\tId = @RecipeId");
        
        if (updateConfig.Title is not null) sqlBuilder.Append(",\n\tTitle = @Title");
        if (updateConfig.Description is not null) sqlBuilder.Append(",\n\tDescription = @Description");
        if (updateConfig.Instruction is not null) sqlBuilder.Append(",\n\tInstruction = @Instruction");
        if (updateConfig.ImageName is not null) sqlBuilder.Append(",\n\tImage_Name = @ImageName");
        if (updateConfig.Difficulty is not null) sqlBuilder.Append(",\n\tDifficulty = @Difficulty");
        if (updateConfig.CookingTime is not null) sqlBuilder.Append(",\n\tCooking_Time = @CookingTime");

        sqlBuilder.Append('\n' + sqlEnding);

        var sql = sqlBuilder.ToString();

        await using var transaction = await db.BeginTransactionAsync();
        
        await db.ExecuteAsync(sqlBuilder.ToString(), new
        {
            @RecipeId = updateConfig.RecipeId.Value,
            @Title = updateConfig.Title?.Value,
            @Description = updateConfig.Description?.Value,
            @Instruction = updateConfig.Instruction?.Value,
            @ImageName = updateConfig.ImageName?.Value,
            @Difficulty = updateConfig.Difficulty?.ToString(),
            @CookingTime = updateConfig.CookingTime
        });

        if (updateConfig.Ingredients is { } ingredients)
        {
            const string deleteIngredientsSql = "DELETE FROM Ingredients WHERE Recipe_Id = @RecipeId;";
            const string insertIngredientBeginning = """
                                                     INSERT INTO Ingredients (Id, Recipe_Id, Name, Count, Unit)
                                                     VALUES (DEFAULT, @RecipeId, @Name, @Count, @Unit);
                                                     """;

            await db.ExecuteAsync(deleteIngredientsSql, new { @RecipeId = updateConfig.RecipeId.Value });

            foreach (var ingredient in ingredients)
            {
                await db.ExecuteAsync(insertIngredientBeginning, new
                {
                    @RecipeId = updateConfig.RecipeId.Value,
                    @Name = ingredient.Name,
                    @Count = ingredient.Count,
                    @Unit = ingredient.UnitType.ToString()
                });
            }
        }

        await transaction.CommitAsync();
    }

    public async Task DeleteAsync(RecipeId recipeId)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        const string sql = "DELETE FROM Recipes WHERE Id = @Id;";
        await db.ExecuteAsync(sql, new { @Id = recipeId.Value });
    }
}