using Domain.RecipeEntity;

namespace Application.Recipes;

public class RecipeGetByIdUseCase
{
    private readonly IRecipeRepository _repo;

    public RecipeGetByIdUseCase(IRecipeRepository repo)
    {
        _repo = repo;
    }

    public async Task<Recipe?> GetRecipeAsync(int id)
    {
        var recipeId = new RecipeId(id);
        return await _repo.SearchByIdAsync(recipeId);
    }
}