using Domain.RecipeEntity;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Recipes.Rate;

public class RecipeRate
{
    private readonly IRecipeRepository _recipeRepo;

    public RecipeRate(IRecipeRepository recipeRepo)
    {
        _recipeRepo = recipeRepo;
    }

    public async Task<Result<Stars>> Rate(RecipeRateDto dto)
    {
        var recipeId = new RecipeId(dto.RecipeId);
        var userId = new UserId(dto.UserId);
        var recipe = await _recipeRepo.SearchByIdAsync(recipeId);
        if (recipe is null) return RecipeErrors.RecipeNotFound;
        if (recipe.Author.Id == userId) return RecipeErrors.UserIsAuthor;
        
        if (dto.Stars is null
            || !Enum.IsDefined((Stars)dto.Stars)
            || dto.Stars == (int)Stars.Zero) return RecipeErrors.StarsAreNotDefined;

        var rate = (Stars)dto.Stars;

        return await _recipeRepo.RateAsync(recipeId, userId, rate);
    }
}