using Application.Users.UseCases;
using Domain.RecipeEntity;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Recipes.Delete;

public class RecipeDelete
{
    private readonly IRecipeRepository _repo;
    private readonly IUserRepository _userRepo;

    public RecipeDelete(IRecipeRepository repo, IUserRepository userRepo)
    {
        _repo = repo;
        _userRepo = userRepo;
    }

    public async Task<Result> DeleteAsync(int recipeId, int userId)
    {
        var typedRecipeId = new RecipeId(recipeId);
        var foundRecipe = await _repo.SearchByIdAsync(typedRecipeId);
        if (foundRecipe is null) return RecipeErrors.RecipeNotFound;

        var typedUserId = new UserId(userId);
        var user = (await _userRepo.SearchByIdAsync(typedUserId))!;
        if (foundRecipe.Author.Id != user.Id && user.Role != UserRole.Admin) return RecipeErrors.UserIsNotAuthor;

        await _repo.DeleteAsync(typedRecipeId);
        return Result.Success();
    }
}