using System.Runtime.CompilerServices;
using SharedKernel;

[assembly: InternalsVisibleTo("Persistence.Tests")]

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

    public static Result<Ingredient> Create(string name, decimal count, int unitType)
    {
        if (name.Length is < 3 or > 50) return RecipeDomainErrors.IngredientNameLengthOutOfRange;

        if (count is <= 0 or >= 1_000_000) return RecipeDomainErrors.IngredientCountOutOfRange;

        if (!Enum.IsDefined((IngredientType)unitType)) return RecipeDomainErrors.IngredientMeasurementUnitIsNotDefined;

        return new Ingredient(name, count, (IngredientType)unitType);
    }
    
    public static Result<Ingredient> Create(string name, decimal count, IngredientType unitType)
    {
        return Create(name, count, (int)unitType);
    }
    
    public static Result<Ingredient> Create(string name, decimal count, string unitType)
    {
        return !Enum.TryParse<IngredientType>(unitType, true, out var unit) 
            ? RecipeDomainErrors.IngredientMeasurementUnitIsNotDefined 
            : Create(name, count, (int)unit);
    }
}