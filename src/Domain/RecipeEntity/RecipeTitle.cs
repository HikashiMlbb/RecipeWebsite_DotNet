using SharedKernel;

namespace Domain.RecipeEntity;

public record RecipeTitle
{
    public string Value { get; init; }

    private RecipeTitle(string value)
    {
        Value = value;
    }

    public static Result<RecipeTitle> Create(string value)
    {
        if (value.Length is < 3 or > 50)
        {
            return new Error();
        }

        return new RecipeTitle(value);
    }
}