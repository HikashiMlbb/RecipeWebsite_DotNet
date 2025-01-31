using Application.Users.UseCases;
using Domain.RecipeEntity;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Recipes.Create;

public class RecipeCreate
{
    private readonly IRecipeRepository _recipeRepo;
    private readonly IUserRepository _userRepo;

    public RecipeCreate(IUserRepository userRepo, IRecipeRepository recipeRepo)
    {
        _userRepo = userRepo;
        _recipeRepo = recipeRepo;
    }


    public async Task<Result<RecipeId>> CreateAsync(RecipeCreateDto dto)
    {
        var authorId = new UserId(dto.AuthorId);
        var foundUser = await _userRepo.SearchByIdAsync(authorId);
        if (foundUser is null) return UserErrors.UserNotFound;

        var recipeTitleResult = RecipeTitle.Create(dto.Title);
        if (!recipeTitleResult.IsSuccess) return recipeTitleResult.Error!;

        var recipeDescriptionResult = RecipeDescription.Create(dto.Description);
        if (!recipeDescriptionResult.IsSuccess) return recipeDescriptionResult.Error!;

        var recipeInstructionResult = RecipeInstruction.Create(dto.Instruction);
        if (!recipeInstructionResult.IsSuccess) return recipeInstructionResult.Error!;

        var imageName = new RecipeImageName(dto.ImageName);
        var isDifficultyDefined = Enum.TryParse<RecipeDifficulty>(dto.Difficulty, true, out var difficulty);
        if (!isDifficultyDefined) return RecipeErrors.DifficultyIsNotDefined;

        var isParseSuccess = TimeSpan.TryParse(dto.CookingTime, out var cookingTime);
        if (!isParseSuccess) return RecipeErrors.CookingTimeHasInvalidFormat;

        if (cookingTime > TimeSpan.FromDays(7)) return RecipeErrors.CookingTimeIsTooHuge;

        if (dto.Ingredients.Count == 0) return RecipeErrors.NoIngredientsProvided;

        var ingredientMappingResult =
            dto.Ingredients.Select(x => Ingredient.Create(x.Name, x.Count, x.UnitType)).ToList();
        var failedIngredient = ingredientMappingResult.FirstOrDefault(x => !x.IsSuccess);
        if (failedIngredient is not null) return failedIngredient.Error!;

        var ingredients = ingredientMappingResult.Select(x => x.Value!).ToList();

        var newRecipe = new Recipe(
            authorId,
            recipeTitleResult.Value!,
            recipeDescriptionResult.Value!,
            recipeInstructionResult.Value!,
            imageName,
            difficulty,
            cookingTime, ingredients: ingredients);

        var result = await _recipeRepo.InsertAsync(newRecipe);
        return result;
    }
}