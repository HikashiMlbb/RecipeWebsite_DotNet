namespace Domain.RecipeEntity;

public sealed record Ingredient(string Name, decimal Count, IngredientType UnitType);