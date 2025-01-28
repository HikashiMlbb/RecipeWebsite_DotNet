using Application.Recipes;
using Application.Recipes.Update;
using Domain.RecipeEntity;
using Domain.UserEntity;

namespace Persistence.Repositories;

public class RecipeRepository : IRecipeRepository
{
    public Task<RecipeId> InsertAsync(Recipe newRecipe)
    {
        throw new NotImplementedException();
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