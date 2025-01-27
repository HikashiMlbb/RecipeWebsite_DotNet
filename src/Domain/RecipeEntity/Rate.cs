namespace Domain.RecipeEntity;

public sealed record Rate(decimal Value, int TotalVotes)
{
    public static readonly Rate Default = new(0, 0);
}