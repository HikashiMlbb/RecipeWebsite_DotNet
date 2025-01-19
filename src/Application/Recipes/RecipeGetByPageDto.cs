namespace Application.Recipes;

public record RecipeGetByPageDto(int Page = 1, int PageSize = 10, int SortType = 0);