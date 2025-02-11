using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record Ingredient
{
    internal Ingredient(string name, decimal count, IngredientType unitType)
    {
        Name = name;
        Count = count;
        UnitType = unitType;
    }

    public string Name { get; init; } = null!;
    public decimal Count { get; init; }
    public IngredientType UnitType { get; init; }

    public static Result<Ingredient> Create(string? name, decimal? count, int? unitType)
    {
        if (name is null || name.Length is < 3 or > 50) return RecipeDomainErrors.IngredientNameLengthOutOfRange;
        if (name.Any(x => x < ' ')) return RecipeDomainErrors.IngredientNameContainsUnallowedSymbol;

        if (count is null or <= 0 or >= 1_000_000) return RecipeDomainErrors.IngredientCountOutOfRange;

        if (unitType is null || !Enum.IsDefined((IngredientType)unitType)) return RecipeDomainErrors.IngredientMeasurementUnitIsNotDefined;

        return new Ingredient(name, count.Value, (IngredientType)unitType);
    }
    
    public static Result<Ingredient> Create(string? name, decimal? count, IngredientType unitType)
    {
        return Create(name, count, (int)unitType);
    }
    
    public static Result<Ingredient> Create(string? name, decimal? count, string? unitType)
    {
        return !Enum.TryParse<IngredientType>(unitType, true, out var unit) 
            ? RecipeDomainErrors.IngredientMeasurementUnitIsNotDefined 
            : Create(name, count, (int)unit);
    }
}