using Domain.RecipeEntity;

namespace Application.Recipes;

public interface IRecipeRepository
{
    public Task<RecipeId> InsertAsync(Recipe newRecipe);
}