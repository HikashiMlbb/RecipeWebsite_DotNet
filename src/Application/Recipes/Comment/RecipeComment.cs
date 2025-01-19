using Domain.RecipeEntity;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Recipes.Comment;

public class RecipeComment
{
    private readonly IRecipeRepository _recipeRepo;

    public RecipeComment(IRecipeRepository recipeRepo)
    {
        _recipeRepo = recipeRepo;
    }

    public async Task<Result> Comment(RecipeCommentDto dto)
    {
        var recipeId = new RecipeId(dto.RecipeId);
        var recipe = await _recipeRepo.SearchByIdAsync(recipeId);
        if (recipe is null) return RecipeErrors.RecipeNotFound;

        var contentResult = Domain.RecipeEntity.Comment.Create(dto.Content);
        if (!contentResult.IsSuccess) return contentResult.Error!;

        var userId = new UserId(dto.UserId);
        await _recipeRepo.CommentAsync(recipeId, userId, contentResult.Value!);
        return Result.Success();
    }
}