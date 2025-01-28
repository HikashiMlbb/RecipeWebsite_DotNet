using System.Data;
using Application.Recipes;
using Application.Recipes.Update;
using Dapper;
using Dapper.Transaction;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Npgsql;

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

    public Task<Recipe?> SearchByIdAsync(RecipeId recipeId)
    {
        throw new NotImplementedException();
    }

    public Task RateAsync(RecipeId recipeId, UserId userId, Stars rate)
    {
        throw new NotImplementedException();
    }

    public Task CommentAsync(RecipeId recipeId, UserId userId, Comment contentResultValue)
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