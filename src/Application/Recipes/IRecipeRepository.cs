using Application.Recipes.GetByPage;
using Application.Recipes.Update;
using Domain.RecipeEntity;
using Domain.UserEntity;

namespace Application.Recipes;

public interface IRecipeRepository
{
    public Task<RecipeId> InsertAsync(Recipe newRecipe);
    public Task<Recipe?> SearchByIdAsync(RecipeId recipeId);
    public Task<Stars> RateAsync(RecipeId recipeId, UserId userId, Stars rate);
    public Task CommentAsync(RecipeId recipeId, Domain.RecipeEntity.Comment comment);
    public Task<IEnumerable<Recipe>> SearchByPageAsync(int page, int pageSize, RecipeSortType sortType);
    public Task<IEnumerable<Recipe>> SearchByQueryAsync(string query);
    public Task UpdateAsync(RecipeUpdateConfig updateConfig);
    public Task DeleteAsync(RecipeId typedRecipeId);
}