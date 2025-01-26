namespace Persistence.Repositories.Dto;

public class CommentDatabaseDto
{
    public int RecipeId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = null!;
}