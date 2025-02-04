using SharedKernel;

namespace Domain.RecipeEntity;

public static class RecipeDomainErrors
{
    public static readonly Error TitleLengthOutOfRange = new("Title.Length", "Title length is out of range.");
    public static readonly Error TitleContainsUnallowedSymbol = new("Title.Unallowed", "Title contains unallowed symbols.");

    public static readonly Error DescriptionLengthOutOfRange =
        new("Description.Length", "Description length is out of range.");

    public static readonly Error InstructionLengthOutOfRange =
        new("Instruction.Length", "Instruction length is out of range.");

    public static readonly Error IngredientNameLengthOutOfRange =
        new("IngredientName.Length", "Ingredient name length is out of range.");

    public static readonly Error IngredientNameContainsUnallowedSymbol =
        new("IngredientName.Unallowed", "Ingredient name contains unallowed symbols.");
    
    public static readonly Error IngredientCountOutOfRange =
        new("Ingredient.Count", "Ingredient count is out of range.");

    public static readonly Error IngredientMeasurementUnitIsNotDefined =
        new("Ingredient.MeasurementUnit", "Ingredient unit of measurement is not defined.");

    public static readonly Error CommentLengthOutOfRange = new("Comment.Length", "Comment length is out of range.");
}