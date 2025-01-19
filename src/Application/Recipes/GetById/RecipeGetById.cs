using Domain.RecipeEntity;

namespace Application.Recipes.GetById;

public class RecipeGetById
{
    private readonly IRecipeRepository _repo;

    public RecipeGetById(IRecipeRepository repo)
    {
        _repo = repo;
    }

    public async Task<Recipe?> GetRecipeAsync(int id)
    {
        var recipeId = new RecipeId(id);
        return await _repo.SearchByIdAsync(recipeId);
    }
}