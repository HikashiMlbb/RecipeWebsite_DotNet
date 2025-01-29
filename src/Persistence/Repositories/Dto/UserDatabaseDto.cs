namespace Persistence.Repositories.Dto;

public record UserDatabaseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
    public ICollection<RecipeDatabaseDto> Recipes { get; set; } = [];
}