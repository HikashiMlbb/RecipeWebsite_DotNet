using Application.Recipes;
using Application.Recipes.Create;
using Application.Users.UseCases;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeCreateTests
{
    private readonly Mock<IRecipeRepository> _recipeRepoMock;
    private readonly RecipeCreate _useCase;
    private readonly Mock<IUserRepository> _userRepoMock;

    public RecipeCreateTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _useCase = new RecipeCreate(_userRepoMock.Object, _recipeRepoMock.Object);
    }
    
    [Fact]
    public async Task TitleIsInvalid_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCreateDto(1, "", "", "", "", "hard", "", []);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.TitleLengthOutOfRange, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task DescriptionIsInvalid_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "", "", "", "hard", "", []);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.DescriptionLengthOutOfRange, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task InstructionIsInvalid_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "", "", "Hard", "", []);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.InstructionLengthOutOfRange, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task DifficultyIsNotDefined_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "what?", "", []);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.DifficultyIsNotDefined, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task CookingTimeIsInvalid_UnrecognizedFormat_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "hard",
            "1234:1234:1234", []);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.CookingTimeHasInvalidFormat, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task CookingTimeIsInvalid_HugeTime_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "hard",
            "99.23:59:59.9990000", []);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.CookingTimeIsTooHuge, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredient_NoIngredientProvided_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "hard",
            "6.23:59:59.9990000", []);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.NoIngredientsProvided, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredientName_ReturnsError()
    {
        // Arrange
        var ingredientDto = new IngredientDto("um", 1, "grams");
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "hard",
            "6.23:59:59.9990000", [ingredientDto]);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientNameLengthOutOfRange, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredientCount_ZeroCount_ReturnsError()
    {
        // Arrange
        var ingredientDto = new IngredientDto("egg", 0, "pieces");
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "Hard",
            "6.23:59:59.9990000", [ingredientDto]);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientCountOutOfRange, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredientCount_TooMany_ReturnsError()
    {
        // Arrange
        var ingredientDto = new IngredientDto("egg", 1_000_000, "pieces");
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "hard",
            "6.23:59:59.9990000", [ingredientDto]);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientCountOutOfRange, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task InvalidIngredientMeasurementUnit_ReturnsError()
    {
        // Arrange
        var ingredientDto = new IngredientDto("egg", 5, "what");
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "hard",
            "6.23:59:59.9990000", [ingredientDto]);

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientMeasurementUnitIsNotDefined, result.Error);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }

    [Fact]
    public async Task SuccessCreating_ReturnsRecipeId()
    {
        // Arrange
        const int recipeId = 666_69;
        var ingredientDto = new IngredientDto("egg", 5, "pieces");
        var dto = new RecipeCreateDto(1, "SomeValidTitle",
            "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", "hard",
            "6.23:59:59.9990000", [ingredientDto]);
        _recipeRepoMock.Setup(x => x.InsertAsync(It.IsAny<Recipe>())).ReturnsAsync(new RecipeId(recipeId));

        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(recipeId, result.Value!.Value);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Once);
    }
}