namespace Persistence.Repositories.Dto;

public class IngredientDatabaseDto
{
    public int RecipeId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Count { get; set; }
    public string UnitType { get; set; } = null!;
}