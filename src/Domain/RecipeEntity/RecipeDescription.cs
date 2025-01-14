using SharedKernel;

namespace Domain.RecipeEntity;

public record RecipeDescription
{
    public string Value { get; init; }

    private RecipeDescription(string value)
    {
        Value = value;
    }

    public static Result<RecipeDescription> Create(string value)
    {
        if (value.Length is < 50 or > 10000)
        {
            return RecipeDomainErrors.DescriptionLengthOutOfRange;
        }

        return new RecipeDescription(value);
    }
}