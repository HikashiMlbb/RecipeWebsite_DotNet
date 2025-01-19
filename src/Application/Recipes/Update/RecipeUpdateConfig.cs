using Domain.RecipeEntity;
using Domain.UserEntity;

namespace Application.Recipes.Update;

public record RecipeUpdateConfig(
    RecipeId RecipeId,
    UserId UserId,
    RecipeTitle? Title = null,
    RecipeDescription? Description = null,
    RecipeInstruction? Instruction = null,
    RecipeImageName? ImageName = null,
    RecipeDifficulty? Difficulty = null,
    TimeSpan? CookingTime = null,
    List<Ingredient>? Ingredients = null);