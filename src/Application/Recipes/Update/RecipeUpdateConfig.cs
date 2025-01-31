using Domain.RecipeEntity;

namespace Application.Recipes.Update;

public record RecipeUpdateConfig(
    RecipeId RecipeId,
    RecipeTitle? Title = null,
    RecipeDescription? Description = null,
    RecipeInstruction? Instruction = null,
    RecipeImageName? ImageName = null,
    RecipeDifficulty? Difficulty = null,
    TimeSpan? CookingTime = null,
    List<Ingredient>? Ingredients = null);