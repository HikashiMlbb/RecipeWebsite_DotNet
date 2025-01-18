using Application.Recipes;
using Application.Users;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Moq;
// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeCreateTests
{
    private readonly RecipeCreateUseCase _useCase;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRecipeRepository> _recipeRepoMock;

    public RecipeCreateTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _recipeRepoMock = new Mock<IRecipeRepository>();
        _useCase = new RecipeCreateUseCase(_userRepoMock.Object, _recipeRepoMock.Object);
    }

    [Fact]
    public async Task UserNotFound_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCreateDto(1, "", "", "", "", 0, "", []);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync((User)null!);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.UserNotFound, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task TitleIsInvalid_ReturnsError()
    {
        // Arrange
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "", "", "", "", 0, "", []);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.TitleLengthOutOfRange, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task DescriptionIsInvalid_ReturnsError()
    {
        // Arrange
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "", "", "", 0, "", []);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.DescriptionLengthOutOfRange, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task InstructionIsInvalid_ReturnsError()
    {
        // Arrange
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "", "", 0, "", []);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.InstructionLengthOutOfRange, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task DifficultyIsNotDefined_ReturnsError()
    {
        // Arrange
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 4, "", []);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.DifficultyIsNotDefined, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task CookingTimeIsInvalid_UnrecognizedFormat_ReturnsError()
    {
        // Arrange
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 2, "1234:1234:1234", []);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.CookingTimeHasInvalidFormat, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task CookingTimeIsInvalid_HugeTime_ReturnsError()
    {
        // Arrange
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 2, "99.23:59:59.9990000", []);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.CookingTimeIsTooHuge, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task InvalidIngredient_NoIngredientProvided_ReturnsError()
    {
        // Arrange
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 2, "6.23:59:59.9990000", []);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.NoIngredientsProvided, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task InvalidIngredientName_ReturnsError()
    {
        // Arrange
        var ingredientDto = new IngredientDto("um", 1, 0);
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 2, "6.23:59:59.9990000", [ingredientDto]);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientNameLengthOutOfRange, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task InvalidIngredientCount_ZeroCount_ReturnsError()
    {
        // Arrange
        var ingredientDto = new IngredientDto("egg", 0, 0);
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 2, "6.23:59:59.9990000", [ingredientDto]);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientCountOutOfRange, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task InvalidIngredientCount_TooMany_ReturnsError()
    {
        // Arrange
        var ingredientDto = new IngredientDto("egg", 1_000_000, 0);
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 2, "6.23:59:59.9990000", [ingredientDto]);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientCountOutOfRange, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task InvalidIngredientMeasurementUnit_ReturnsError()
    {
        // Arrange
        var ingredientDto = new IngredientDto("egg", 5, -1);
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 2, "6.23:59:59.9990000", [ingredientDto]);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeDomainErrors.IngredientMeasurementUnitIsNotDefined, result.Error);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Never);
    }
    
    [Fact]
    public async Task SuccessCreating_ReturnsRecipeId()
    {
        // Arrange
        const int recipeId = 666_69;
        var ingredientDto = new IngredientDto("egg", 5, (int)IngredientType.Pieces);
        var userMock = new User(Username.Create("SomeUsername").Value!, new Password("SomePasswordHashed"));
        var dto = new RecipeCreateDto(1, "SomeValidTitle", "SomeValidDescriptionSomeValidDescriptionSomeValidDescription", "SomeValidInstruction", "", 2, "6.23:59:59.9990000", [ingredientDto]);
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(userMock);
        _recipeRepoMock.Setup(x => x.InsertAsync(It.IsAny<Recipe>())).ReturnsAsync(new RecipeId(recipeId));
        
        // Act
        var result = await _useCase.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(recipeId, result.Value!.Value);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _recipeRepoMock.Verify(x => x.InsertAsync(It.IsAny<Recipe>()), Times.Once);
    }
}