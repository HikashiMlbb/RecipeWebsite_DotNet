using SharedKernel;

namespace Application.Recipes;

public static class RecipeErrors
{
    public static readonly Error RecipeNotFound = new("Recipe.NotFound", "Recipe with given ID has not been found.");

    public static readonly Error DifficultyIsNotDefined =
        new("Recipe.Difficulty", "Given recipe difficulty is not defined.");

    public static readonly Error CookingTimeHasInvalidFormat =
        new("Recipe.CookingTime", "Given recipe cooking time format is not recognized.");

    public static readonly Error CookingTimeIsTooHuge =
        new("Recipe.CookingTime", "Given recipe cooking time is too huge.");

    public static readonly Error CookingTimeIsTooSmall =
        new("Recipe.CookingTime", "Given recipe cooking time is too small.");

    public static readonly Error NoIngredientsProvided =
        new("Recipe.NoIngredients", "There are no one ingredient provided.");

    public static readonly Error StarsAreNotDefined = new("Recipe.Stars", "Given recipe star rate is not defined.");

    public static readonly Error UserIsNotAuthor =
        new("Recipe.Author", "The User is not Author of the Recipe to be able to change it.");

    public static readonly Error UserIsAuthor =
        new("Recipe.Author", "The User is Author of the Recipe to be able to rate it.");
}