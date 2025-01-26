namespace Persistence.Repositories.Dto;

public class RecipeDatabaseDto
{
    public int RecipeId { get; set; }
    public int AuthorId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Instruction { get; set; } = null!;
    public string ImageName { get; set; } = null!;
    public string Difficulty { get; set; } = null!;
    public DateTimeOffset PublishedAt { get; set; }
    public TimeSpan CookingTime { get; set; }
    public decimal Rating { get; set; }
    public int Votes { get; set; }
    public ICollection<IngredientDatabaseDto> Ingredients { get; set; } = [];
    public ICollection<CommentDatabaseDto> Comments { get; set; } = [];
}
    