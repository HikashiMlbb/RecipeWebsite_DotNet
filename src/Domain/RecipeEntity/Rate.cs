namespace Domain.RecipeEntity;

public sealed record Rate(decimal Value, Stars TotalVotes)
{
    public static readonly Rate Default = new(0, Stars.Zero);
}