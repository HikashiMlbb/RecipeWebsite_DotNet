using Application.Recipes;
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

    public Task CommentAsync(RecipeId recipeId, Comment contentResultValue)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Recipe>> SearchByPageAsync(int dtoPage, int dtoPageSize, int dtoSortType)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Recipe>> SearchByQueryAsync(string query)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(RecipeUpdateConfig updateConfig)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(RecipeId typedRecipeId)
    {
        throw new NotImplementedException();
    }
}