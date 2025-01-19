using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record RecipeInstruction
{
    private RecipeInstruction(string value)
    {
        Value = value;
    }

    public string Value { get; init; }

    public static Result<RecipeInstruction> Create(string value)
    {
        if (value.Length is < 10 or > 10000) return RecipeDomainErrors.InstructionLengthOutOfRange;

        return new RecipeInstruction(value);
    }
}