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

        if (!Enum.IsDefined((RecipeSortType)dto.SortType)) dto = dto with { SortType = (int)RecipeSortType.Popular };

        return await _repo.SearchByPageAsync(dto.Page, dto.PageSize, dto.SortType);
    }
}