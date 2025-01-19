using Application.Recipes;
using Application.Recipes.Update;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeUpdateTests
{
    private readonly Mock<IRecipeRepository> _mockRepo;
    private readonly RecipeUpdate _useCase;

    public RecipeUpdateTests()
    {
        _mockRepo = new Mock<IRecipeRepository>();
        _useCase = new RecipeUpdate(_mockRepo.Object);
    }

    [Fact]
    public async Task RecipeNotFound_ReturnsError()
    {
        // Arrange
        var dto = new RecipeUpdateDto(1, 2);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync((Recipe)null!);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.RecipeNotFound, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task UserIsNotAuthor_ReturnsError()
    {
        // Arrange
        const int recipeId = 69;
        const int userId = 666;
        var returnedRecipe = new Recipe { AuthorId = new UserId(70) };
        var dto = new RecipeUpdateDto(recipeId, userId);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>()))
            .ReturnsAsync(returnedRecipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.UserIsNotAuthor, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidTitle_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, "I");
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.TitleLengthOutOfRange, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidDescription_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, Description: "?");
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.DescriptionLengthOutOfRange, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidInstruction_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, Instruction: "?");
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.InstructionLengthOutOfRange, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidDifficulty_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, Difficulty: 3);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.DifficultyIsNotDefined, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidCookingTime_Format_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, CookingTime: "-1:-1:-1:-1");
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.CookingTimeHasInvalidFormat, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidCookingTime_TooHuge_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, CookingTime: "7.0:0:0");
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.CookingTimeIsTooHuge, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidCookingTime_TooSmall_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, CookingTime: "-1");
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.CookingTimeIsTooSmall, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredient_NoIngredientProvided_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, Ingredients: []);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.NoIngredientsProvided, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredientName_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, Ingredients: [new IngredientDto(".", 1, 0)]);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientNameLengthOutOfRange, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredientCount_ZeroCount_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, Ingredients: [new IngredientDto("egg", 0, 0)]);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientCountOutOfRange, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredientCount_TooMany_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, Ingredients: [new IngredientDto("egg", 1_000_000, 0)]);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientCountOutOfRange, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredient_MeasurementUnit_ReturnsError()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(13, 26, Ingredients: [new IngredientDto("egg", 1_000, -1)]);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientMeasurementUnitIsNotDefined, result.Error);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Never);
    }

    [Fact]
    public async Task FullUpdateSuccessfully_ReturnsSuccess()
    {
        // Arrange
        var recipe = new Recipe { AuthorId = new UserId(26) };
        var dto = new RecipeUpdateDto(
            13,
            26,
            "ValidRecipeTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription",
            "SomeValidInstruction",
            "newImageNameGUID",
            2,
            "12:00",
            [new IngredientDto("egg", 1_000, 0)]);
        _mockRepo.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepo.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<RecipeUpdateConfig>()), Times.Once);
    }
}