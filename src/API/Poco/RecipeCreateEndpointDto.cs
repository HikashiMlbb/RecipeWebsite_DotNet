using Application.Recipes;

namespace API.Poco;

public class RecipeCreateEndpointDto
{
    public string? Title { get; init; } = null;
    public string? Description { get; init; } = null;
    public string? Instruction { get; init; } = null;
    public string? Difficulty { get; init; } = null;
    public string? CookingTime { get; init; } = null;
    public List<IngredientDto>? Ingredients { get; init; } = null;
}