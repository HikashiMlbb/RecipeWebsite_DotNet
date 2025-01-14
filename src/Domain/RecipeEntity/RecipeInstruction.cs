using SharedKernel;

namespace Domain.RecipeEntity;

public record RecipeInstruction
{
    public string Value { get; init; }

    private RecipeInstruction(string value)
    {
        Value = value;
    }

    public static Result<RecipeInstruction> Create(string value)
    {
        if (value.Length is < 10 or > 10000)
        {
            return new Error();
        }

        return new RecipeInstruction(value);
    }
}