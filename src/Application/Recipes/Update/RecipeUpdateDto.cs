namespace Application.Recipes.Update;

public record RecipeUpdateDto(
    int RecipeId,
    int UserId,
    string? Title = null,
    string? Description = null,
    string? Instruction = null,
    string? ImageName = null,
    int? Difficulty = null,
    string? CookingTime = null,
    List<IngredientDto>? Ingredients = null);