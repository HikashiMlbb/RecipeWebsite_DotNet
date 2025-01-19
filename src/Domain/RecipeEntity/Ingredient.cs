using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record Ingredient
{
    private Ingredient(string name, decimal count, int unitType)
    {
        Name = name;
        Count = count;
        UnitType = (IngredientType)unitType;
    }

    public string Name { get; init; } = null!;
    public decimal Count { get; init; }
    public IngredientType UnitType { get; init; }

    public static Result<Ingredient> Create(string name, decimal count, int unitType)
    {
        if (name.Length is < 3 or > 50) return RecipeDomainErrors.IngredientNameLengthOutOfRange;

        if (count is <= 0 or >= 1_000_000) return RecipeDomainErrors.IngredientCountOutOfRange;

        if (!Enum.IsDefined((IngredientType)unitType)) return RecipeDomainErrors.IngredientMeasurementUnitIsNotDefined;

        return new Ingredient(name, count, unitType);
    }
}