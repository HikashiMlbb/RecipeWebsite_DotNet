namespace Persistence.Repositories.Dto;

public record RecipeAuthorDto
{
    public int AuthorId { get; init; }
    public string AuthorUsername { get; init; } = null!;
}