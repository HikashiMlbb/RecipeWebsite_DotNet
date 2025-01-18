using SharedKernel;

namespace Application.Recipes;

public static class RecipeErrors
{
    public static readonly Error RecipeNotFound = new("Recipe.NotFound", "Recipe with given ID has not been found.");
    public static readonly Error DifficultyIsNotDefined = new("Recipe.Difficulty", "Given recipe difficulty is not defined.");
    public static readonly Error CookingTimeHasInvalidFormat = new("Recipe.CookingTime", "Given recipe cooking time format is not recognized.");
    public static readonly Error CookingTimeIsTooHuge = new("Recipe.CookingTime", "Given recipe cooking time is too huge.");
    public static readonly Error NoIngredientsProvided = new("Recipe.NoIngredients", "There are no one ingredient provided.");
    public static readonly Error StarsAreNotDefined = new("Recipe.Stars", "Given recipe star rate is not defined.");
}