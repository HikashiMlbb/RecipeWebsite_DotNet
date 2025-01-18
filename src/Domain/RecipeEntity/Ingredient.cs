using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record Ingredient
{
    public string Name { get; init; } = null!;
    public decimal Count { get; init; }
    public IngredientType UnitType { get; init; }

    private Ingredient(string name, decimal count, int unitType)
    {
        Name = name;
        Count = count;
        UnitType = (IngredientType)unitType;
    }

    public static Result<Ingredient> Create(string name, decimal count, int unitType)
    {
        if (name.Length is < 3 or > 50)
        {
            return new Error();
        }

        if (count is <= 0 or >= 1_000_000)
        {
            return new Error();
        }

        if (!Enum.IsDefined((IngredientType)unitType))
        {
            return new Error();
        }

        return new Ingredient(name, count, unitType);
    }
}