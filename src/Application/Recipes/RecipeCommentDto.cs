namespace Application.Recipes;

public record RecipeCommentDto(int UserId, int RecipeId, string Content);