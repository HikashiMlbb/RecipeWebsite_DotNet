using Domain.RecipeEntity;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Recipes.Update;

public class RecipeUpdate
{
    private readonly IRecipeRepository _recipeRepo;

    public RecipeUpdate(IRecipeRepository recipeRepo)
    {
        _recipeRepo = recipeRepo;
    }

    public async Task<Result> UpdateAsync(RecipeUpdateDto dto)
    {
        var recipeId = new RecipeId(dto.RecipeId);
        var foundRecipe = await _recipeRepo.SearchByIdAsync(recipeId);
        if (foundRecipe is null) return RecipeErrors.RecipeNotFound;

        var userId = new UserId(dto.UserId);
        if (userId != foundRecipe.AuthorId) return RecipeErrors.UserIsNotAuthor;

        var updateConfig = new RecipeUpdateConfig(recipeId, userId);

        if (dto.Title is { } title)
        {
            var titleResult = RecipeTitle.Create(title);
            if (!titleResult.IsSuccess) return titleResult.Error!;

            updateConfig = updateConfig with { Title = titleResult.Value! };
        }

        if (dto.Description is { } description)
        {
            var descriptionResult = RecipeDescription.Create(description);
            if (!descriptionResult.IsSuccess) return descriptionResult.Error!;

            updateConfig = updateConfig with { Description = descriptionResult.Value! };
        }

        if (dto.Instruction is { } instruction)
        {
            var instructionResult = RecipeInstruction.Create(instruction);
            if (!instructionResult.IsSuccess) return instructionResult.Error!;

            updateConfig = updateConfig with { Instruction = instructionResult.Value! };
        }

        if (dto.ImageName is { } imageName)
            updateConfig = updateConfig with { ImageName = new RecipeImageName(imageName) };

        if (dto.Difficulty is { } rawDifficulty)
        {
            if (!Enum.TryParse<RecipeDifficulty>(rawDifficulty, true, out var difficulty)) return RecipeErrors.DifficultyIsNotDefined;

            updateConfig = updateConfig with { Difficulty = difficulty };
        }

        if (dto.CookingTime is { } rawCookingTime)
        {
            var isParseSuccess = TimeSpan.TryParse(rawCookingTime, out var cookingTime);
            if (!isParseSuccess) return RecipeErrors.CookingTimeHasInvalidFormat;

            if (cookingTime >= TimeSpan.FromDays(7)) return RecipeErrors.CookingTimeIsTooHuge;

            if (cookingTime < TimeSpan.Zero) return RecipeErrors.CookingTimeIsTooSmall;

            updateConfig = updateConfig with { CookingTime = cookingTime };
        }

        if (dto.Ingredients is { } rawIngredients)
        {
            if (rawIngredients.Count == 0) return RecipeErrors.NoIngredientsProvided;

            var ingredientMappingResult =
                dto.Ingredients.Select(x => Ingredient.Create(x.Name, x.Count, x.UnitType)).ToList();
            var failedIngredient = ingredientMappingResult.FirstOrDefault(x => !x.IsSuccess);
            if (failedIngredient is not null) return failedIngredient.Error!;

            var ingredients = ingredientMappingResult.Select(x => x.Value!).ToList();
            updateConfig = updateConfig with { Ingredients = ingredients };
        }

        await _recipeRepo.UpdateAsync(updateConfig);
        return Result.Success();
    }
}