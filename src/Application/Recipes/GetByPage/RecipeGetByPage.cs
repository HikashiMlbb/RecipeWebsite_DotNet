using Domain.RecipeEntity;

namespace Application.Recipes.GetByPage;

public class RecipeGetByPage
{
    private readonly IRecipeRepository _repo;

    public RecipeGetByPage(IRecipeRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Recipe>> GetRecipesAsync(RecipeGetByPageDto dto)
    {
        if (dto.Page <= 0) dto = dto with { Page = 1 };

        if (dto.PageSize <= 0) dto = dto with { PageSize = 10 };

        if (!Enum.TryParse<RecipeSortType>(dto.SortType, true, out _)) dto = dto with { SortType = RecipeSortType.Popular.ToString() };

        return await _repo.SearchByPageAsync(dto.Page, dto.PageSize, Enum.Parse<RecipeSortType>(dto.SortType, ignoreCase: true));
    }
}