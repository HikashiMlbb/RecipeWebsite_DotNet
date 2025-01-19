using Application.Recipes.Update;
using Domain.RecipeEntity;
using Domain.UserEntity;

namespace Application.Recipes;

public interface IRecipeRepository
{
    public Task<RecipeId> InsertAsync(Recipe newRecipe);
    public Task<Recipe?> SearchByIdAsync(RecipeId recipeId);
    public Task RateAsync(RecipeId recipeId, UserId userId, Stars rate);
    public Task CommentAsync(RecipeId recipeId, UserId userId, Domain.RecipeEntity.Comment contentResultValue);
    public Task<IEnumerable<Recipe>> SearchByPageAsync(int dtoPage, int dtoPageSize, int dtoSortType);
    public Task<IEnumerable<Recipe>> SearchByQueryAsync(string query);
    public Task UpdateAsync(RecipeUpdateConfig updateConfig);
    public Task DeleteAsync(RecipeId typedRecipeId);
}