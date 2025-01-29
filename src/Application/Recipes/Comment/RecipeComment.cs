using Application.Users.UseCases;
using Domain.RecipeEntity;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Recipes.Comment;

public class RecipeComment
{
    private readonly IRecipeRepository _recipeRepo;
    private IUserRepository _userRepo;

    public RecipeComment(IRecipeRepository recipeRepo, IUserRepository userRepo)
    {
        _recipeRepo = recipeRepo;
        _userRepo = userRepo;
    }

    public async Task<Result> Comment(RecipeCommentDto dto)
    {
        var recipeId = new RecipeId(dto.RecipeId);
        var recipe = await _recipeRepo.SearchByIdAsync(recipeId);
        if (recipe is null) return RecipeErrors.RecipeNotFound;
        
        var userId = new UserId(dto.UserId);
        var user = await _userRepo.SearchByIdAsync(userId);
        var commentResult = Domain.RecipeEntity.Comment.Create(user!, dto.Content);
        if (!commentResult.IsSuccess) return commentResult.Error!;

        await _recipeRepo.CommentAsync(recipeId, commentResult.Value!);
        return Result.Success();
    }
}