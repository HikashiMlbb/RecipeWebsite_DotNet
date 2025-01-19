using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record RecipeDescription
{
    private RecipeDescription(string value)
    {
        Value = value;
    }

    public string Value { get; init; }

    public static Result<RecipeDescription> Create(string value)
    {
        if (value.Length is < 50 or > 10000) return RecipeDomainErrors.DescriptionLengthOutOfRange;

        return new RecipeDescription(value);
    }
}