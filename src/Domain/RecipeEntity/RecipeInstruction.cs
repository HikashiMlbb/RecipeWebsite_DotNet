using System.Runtime.CompilerServices;
using SharedKernel;

[assembly: InternalsVisibleTo("Persistence.Tests")]

namespace Domain.RecipeEntity;

public sealed record RecipeInstruction
{
    internal RecipeInstruction(string value)
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