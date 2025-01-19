using Domain.RecipeEntity;

namespace Application.Recipes;

public class RecipeGetByQueryUseCase
{
    private readonly IRecipeRepository _repo;

    public RecipeGetByQueryUseCase(IRecipeRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Recipe>> GetRecipesAsync(string query)
    {
        return await _repo.SearchByQueryAsync(query);
    }
}