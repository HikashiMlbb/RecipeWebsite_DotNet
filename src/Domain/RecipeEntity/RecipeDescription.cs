using System.Runtime.CompilerServices;
using SharedKernel;

[assembly: InternalsVisibleTo("Persistence.Tests")]

namespace Domain.RecipeEntity;

public sealed record RecipeDescription
{
    internal RecipeDescription(string value)
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