using Domain.RecipeEntity;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Recipes;

public class RecipeDeleteUseCase
{
    private readonly IRecipeRepository _repo;

    public RecipeDeleteUseCase(IRecipeRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result> DeleteAsync(int recipeId, int userId)
    {
        var typedRecipeId = new RecipeId(recipeId);
        var foundRecipe = await _repo.SearchByIdAsync(typedRecipeId);
        if (foundRecipe is null)
        {
            return RecipeErrors.RecipeNotFound;
        }

        var typedUserId = new UserId(userId);
        if (foundRecipe.AuthorId != typedUserId)
        {
            return RecipeErrors.UserIsNotAuthor;
        }

        await _repo.DeleteAsync(typedRecipeId);
        return Result.Success();
    } 
}