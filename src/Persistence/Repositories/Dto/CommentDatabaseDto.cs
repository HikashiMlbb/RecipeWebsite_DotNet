namespace Persistence.Repositories.Dto;

public record CommentDatabaseDto
{
    public long CommentId { get; set; }
    public UserDatabaseDto Author { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTimeOffset CommentPublishedAt { get; set; }
}