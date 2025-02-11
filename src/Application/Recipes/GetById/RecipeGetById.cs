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
        var foundRecipe = await _repo.SearchByIdAsync(recipeId);
        if (foundRecipe is null) return null;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        foundRecipe.Comments = foundRecipe.Comments is null
            ? []
            : foundRecipe.Comments.OrderByDescending(x => x.PublishedAt).ToList();
        return foundRecipe;
    }
}