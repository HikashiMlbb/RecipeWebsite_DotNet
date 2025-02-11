namespace Application.Recipes.GetByPage;

public record RecipeGetByPageDto(int Page = 1, int PageSize = 10, string SortType = "popular");