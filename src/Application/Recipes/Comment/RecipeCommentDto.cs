namespace Application.Recipes.Comment;

public record RecipeCommentDto(int UserId, int RecipeId, string Content);