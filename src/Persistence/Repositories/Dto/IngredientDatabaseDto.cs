namespace Persistence.Repositories.Dto;

public record IngredientDatabaseDto
{
    public long IngredientId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Count { get; set; }
    public string UnitType { get; set; } = null!;
}