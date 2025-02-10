using System.Text;
using Application.Recipes;
using Application.Recipes.GetByPage;
using Application.Recipes.Update;
using Dapper;
using Dapper.Transaction;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Persistence.Repositories.Dto;
using Z.Dapper.Plus;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Persistence.Repositories;

public class RecipeRepository(DapperConnectionFactory factory) : IRecipeRepository
{
    public async Task<RecipeId> InsertAsync(Recipe newRecipe)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        await using var transaction = await db.BeginTransactionAsync();

        const string recipeSql = """
                                 INSERT INTO "Recipes" ("Id", "AuthorId", "Title", "Description", "Instruction", "ImageName", "Difficulty", "PublishedAt", "CookingTime", "Rating", "Votes")
                                 VALUES (DEFAULT, @UserId, @Title, @Description, @Instruction, @ImageName, @Difficulty, @PublishedAt, @CookingTime, @Rating, @Votes)
                                 RETURNING "Id";
                                 """;

        var recipeId = await transaction.QueryFirstAsync<int>(recipeSql, new
        {
            UserId = newRecipe.Author.Id.Value,
            Title = newRecipe.Title.Value,
            Description = newRecipe.Description.Value,
            Instruction = newRecipe.Instruction.Value,
            ImageName = newRecipe.ImageName.Value,
            Difficulty = newRecipe.Difficulty.ToString(),
            PublishedAt = newRecipe.PublishedAt.ToUniversalTime(),
            CookingTime = newRecipe.CookingTime,
            Rating = 0,
            Votes = 0
        });
        
        await transaction.UseBulkOptions(x => x.DestinationTableName = "Ingredients").BulkInsertAsync(newRecipe.Ingredients.Select(x => new
        {
            RecipeId = recipeId,
            Name = x.Name,
            Count = x.Count,
            Unit = x.UnitType.ToString()
        }));

        await transaction.CommitAsync();

        return new RecipeId(recipeId);
    }

    public async Task<Recipe?> SearchByIdAsync(RecipeId recipeId)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        const string sql = """
                           SELECT 
                                recipes."Id" AS "RecipeId",
                                recipes."Title",
                                recipes."Description",
                                recipes."Instruction",
                                recipes."ImageName",
                                recipes."Difficulty",
                                recipes."PublishedAt",
                                recipes."CookingTime",
                                recipes."Rating",
                                recipes."Votes",
                                recipe_author."Id" AS "AuthorId",
                                recipe_author."Username" AS "AuthorUsername",
                                ingredients."Id" AS "IngredientId",
                                ingredients."Name",
                                ingredients."Count",
                                ingredients."Unit" AS "UnitType",
                                comments."Id" AS "CommentId",
                                comments."Content",
                                comments."PublishedAt" AS "CommentPublishedAt",
                                comment_author."Id" AS "CommentAuthorId",
                                comment_author."Username" AS "CommentAuthorUsername"
                           FROM "Recipes" recipes
                           LEFT OUTER JOIN "Users" recipe_author ON recipe_author."Id" = recipes."AuthorId"
                           LEFT OUTER JOIN "Ingredients" ingredients ON ingredients."RecipeId" = recipes."Id"
                           LEFT OUTER JOIN "Comments" comments ON comments."RecipeId" = recipes."Id"
                           LEFT OUTER JOIN "Users" comment_author ON comment_author."Id" = comments."UserId"
                           WHERE recipes."Id" = @Id;
                           """;

        RecipeDatabaseDto? detailedDto = null;

        var result = 
            (await db.QueryAsync<RecipeDatabaseDto, RecipeAuthorDto, IngredientDatabaseDto?, CommentDatabaseDto?, CommentAuthorDto, RecipeDatabaseDto>(
                sql,
                (recipeDto, recipeAuthorDto, ingredientDto, commentDto, commentAuthorDto) =>
                {
                    detailedDto ??= recipeDto;
                    detailedDto.Author = recipeAuthorDto;

                    if (ingredientDto is not null && detailedDto.Ingredients.IsAbsent(ingredientDto.IngredientId))
                    {
                        detailedDto.Ingredients.Add(ingredientDto);
                    }

                    if (commentDto is not null && detailedDto.Comments.IsAbsent(commentDto.CommentId))
                    {
                        detailedDto.Comments.Add(commentDto with { Author = commentAuthorDto });
                    }

                    return detailedDto;
                },
                new
                {
                    @Id = recipeId.Value
                }, 
                splitOn: "AuthorId, IngredientId, CommentId, CommentAuthorId")).ToList();

        if (result.Count == 0) return null;

        var uniqueResult = result.Distinct().Single();


        ICollection<Ingredient> ingredients = uniqueResult.Ingredients.Select(x => 
            new Ingredient(x.Name, x.Count, Enum.Parse<IngredientType>(x.UnitType, ignoreCase: true))).AsList();

        ICollection<Comment> comments = uniqueResult.Comments.Select(x =>
            Comment.Create(
                new User { Id = new UserId(x.Author.CommentAuthorId), Username = Username.Create(x.Author.CommentAuthorUsername).Value! },
                x.Content, x.CommentPublishedAt).Value!).AsList();

        return new Recipe
        {
            Id = new RecipeId(uniqueResult.RecipeId),
            Author = new User
            {
                Id = new UserId(uniqueResult.Author.AuthorId),
                Username = new Username(uniqueResult.Author.AuthorUsername)
            },
            Title = RecipeTitle.Create(uniqueResult.Title).Value!,
            Description = RecipeDescription.Create(uniqueResult.Description).Value!,
            Instruction = RecipeInstruction.Create(uniqueResult.Instruction).Value!,
            ImageName = new RecipeImageName(uniqueResult.ImageName),
            Difficulty = Enum.Parse<RecipeDifficulty>(uniqueResult.Difficulty, true),
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

        const string sql = "SELECT \"RateRecipe\"(@RecipeId, @UserId, @Rate)";

        var newRate = await db.QueryFirstAsync<int>(sql, new
        {
            RecipeId = recipeId.Value,
            UserId = userId.Value,
            Rate = (int)rate
        });

        return (Stars)newRate;
    }

    public async Task CommentAsync(RecipeId recipeId, Comment comment)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        const string sql = """
                           INSERT INTO "Comments" ("Id", "RecipeId", "UserId", "Content", "PublishedAt")
                           VALUES (DEFAULT, @RecipeId, @UserId, @Content, @PublishedAt);
                           """;
        await db.ExecuteAsync(sql, new
        {
            RecipeId = recipeId.Value,
            UserId = comment.Author.Id.Value,
            comment.Content,
            comment.PublishedAt
        });
    }

    public async Task<IEnumerable<Recipe>> SearchByPageAsync(int page, int pageSize, RecipeSortType sortType)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        const string sqlBeginning = """
                                    SELECT
                                         "Id" AS "RecipeId",
                                         "Title",
                                         "ImageName",
                                         "Difficulty",
                                         "CookingTime",
                                         "Rating",
                                         "Votes"
                                    FROM "Recipes"

                                    """;

        const string sqlEnding = """

                                 LIMIT @Limit
                                 OFFSET @Offset;
                                 """;

        const string popular = """
                               ORDER BY
                                    "Votes" DESC,
                                    "Rating" DESC
                               """;

        const string newest = "ORDER BY \"PublishedAt\" DESC";

        var chosenSort = sortType switch
        {
            RecipeSortType.Popular => popular,
            RecipeSortType.Newest => newest,
            _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null)
        };

        var sql = sqlBeginning + chosenSort + sqlEnding;

        var foundRawRecipes = await db.QueryAsync<RecipeDatabaseDto>(sql, new
        {
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        });

        return foundRawRecipes.Select(x => new Recipe
        {
            Id = new RecipeId(x.RecipeId),
            Title = new RecipeTitle(x.Title),
            ImageName = new RecipeImageName(x.ImageName),
            Difficulty = Enum.Parse<RecipeDifficulty>(x.Difficulty, true),
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
                                "Id" AS "RecipeId",
                                "Title",
                                "ImageName",
                                "Difficulty",
                                "CookingTime",
                                "Rating",
                                "Votes"
                           FROM "Recipes"
                           WHERE LOWER("Title") LIKE LOWER(@Query) OR LOWER("Description") LIKE LOWER(@Query)
                           ORDER BY 
                               CASE
                                   WHEN LOWER("Title") LIKE LOWER(@Query) THEN 1
                                   ELSE 2
                               END;
                           """;

        var result = await db.QueryAsync<RecipeDatabaseDto>(sql, new
        {
            Query = $"%{query}%"
        });

        return result.Select(x => new Recipe
        {
            Id = new RecipeId(x.RecipeId),
            Title = new RecipeTitle(x.Title),
            ImageName = new RecipeImageName(x.ImageName),
            Difficulty = Enum.Parse<RecipeDifficulty>(x.Difficulty, true),
            CookingTime = x.CookingTime,
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

        await using var transaction = await db.BeginTransactionAsync();

        await db.ExecuteAsync(sqlBuilder.ToString(), new
        {
            RecipeId = updateConfig.RecipeId.Value,
            Title = updateConfig.Title?.Value,
            Description = updateConfig.Description?.Value,
            Instruction = updateConfig.Instruction?.Value,
            ImageName = updateConfig.ImageName?.Value,
            Difficulty = updateConfig.Difficulty?.ToString(),
            updateConfig.CookingTime
        });

        if (updateConfig.Ingredients is { } ingredients)
        {
            const string deleteIngredientsSql = "DELETE FROM Ingredients WHERE Recipe_Id = @RecipeId;";

            await db.ExecuteAsync(deleteIngredientsSql, new { RecipeId = updateConfig.RecipeId.Value });

            await db.UseBulkOptions(x => x.DestinationTableName = "ingredients").BulkInsertAsync(ingredients.Select(x => new
            {
                Recipe_Id = updateConfig.RecipeId.Value,
                Name = x.Name,
                Count = x.Count,
                Unit = x.UnitType.ToString()
            }));
        }

        await transaction.CommitAsync();
    }

    public async Task DeleteAsync(RecipeId recipeId)
    {
        await using var db = factory.Create();
        await db.OpenAsync();

        const string sql = "DELETE FROM Recipes WHERE Id = @Id;";
        await db.ExecuteAsync(sql, new { Id = recipeId.Value });
    }
}