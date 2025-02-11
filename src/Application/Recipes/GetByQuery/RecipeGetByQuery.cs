using Domain.RecipeEntity;

namespace Application.Recipes.GetByQuery;

public class RecipeGetByQuery
{
    private readonly IRecipeRepository _repo;

    public RecipeGetByQuery(IRecipeRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Recipe>> GetRecipesAsync(string query)
    {
        return await _repo.SearchByQueryAsync(query);
    }
}