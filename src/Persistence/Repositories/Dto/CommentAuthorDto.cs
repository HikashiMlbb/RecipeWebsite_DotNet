namespace Persistence.Repositories.Dto;

public record CommentAuthorDto
{
    public int CommentAuthorId { get; init; }
    public string CommentAuthorUsername { get; init; } = null!;
}