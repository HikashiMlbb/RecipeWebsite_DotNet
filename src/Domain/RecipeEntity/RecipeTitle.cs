using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record RecipeTitle
{
    private RecipeTitle(string value)
    {
        Value = value;
    }

    public string Value { get; init; }

    public static Result<RecipeTitle> Create(string value)
    {
        if (value.Length is < 3 or > 50) return RecipeDomainErrors.TitleLengthOutOfRange;

        return new RecipeTitle(value);
    }
}