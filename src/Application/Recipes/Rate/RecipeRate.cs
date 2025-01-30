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
        var userId = new UserId(dto.UserId);
        var recipe = await _recipeRepo.SearchByIdAsync(recipeId);
        if (recipe is null) return RecipeErrors.RecipeNotFound;
        if (recipe.AuthorId == userId) return RecipeErrors.UserIsAuthor; 

        var areStarsDefined = Enum.IsDefined((Stars)dto.Stars);
        if (!areStarsDefined || dto.Stars == (int)Stars.Zero) return RecipeErrors.StarsAreNotDefined;

        var rate = (Stars)dto.Stars;

        await _recipeRepo.RateAsync(recipeId, userId, rate);

        return Result.Success();
    }
}