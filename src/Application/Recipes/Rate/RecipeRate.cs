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

    public async Task<Result> Rate(RecipeRateDto dto)
    {
        var recipeId = new RecipeId(dto.RecipeId);
        var recipe = await _recipeRepo.SearchByIdAsync(recipeId);
        if (recipe is null) return RecipeErrors.RecipeNotFound;

        var areStarsDefined = Enum.IsDefined((Stars)dto.Stars);
        if (!areStarsDefined || dto.Stars == 0) return RecipeErrors.StarsAreNotDefined;

        var rate = (Stars)dto.Stars;
        var userId = new UserId(dto.UserId);

        await _recipeRepo.RateAsync(recipeId, userId, rate);

        return Result.Success();
    }
}