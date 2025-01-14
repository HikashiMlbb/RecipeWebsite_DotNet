using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record RecipeInstruction
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
            return RecipeDomainErrors.InstructionLengthOutOfRange;
        }

        return new RecipeInstruction(value);
    }
}