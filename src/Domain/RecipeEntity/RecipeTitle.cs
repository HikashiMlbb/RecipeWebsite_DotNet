using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record RecipeTitle
{
    internal RecipeTitle(string value)
    {
        Value = value;
    }

    public string Value { get; init; }

    public static Result<RecipeTitle> Create(string? value)
    {
        if (value is null || value.Length is < 3 or > 50) return RecipeDomainErrors.TitleLengthOutOfRange;
        if (value.Any(x => x < ' ')) return RecipeDomainErrors.TitleContainsUnallowedSymbol;

        return new RecipeTitle(value);
    }
}