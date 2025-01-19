using Application.Recipes;
using Domain.RecipeEntity;
using Moq;
// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeGetByIdTests
{
    private readonly Mock<IRecipeRepository> _repoMock;
    private readonly RecipeGetByIdUseCase _useCase;

    public RecipeGetByIdTests()
    {
        _repoMock = new Mock<IRecipeRepository>();
        _useCase = new RecipeGetByIdUseCase(_repoMock.Object);
    }

    [Fact]
    public async Task RecipeNotFound_ReturnsError()
    {
        // Arrange
        const int id = 16;
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync((Recipe)null!);
        
        // Act
        var result = await _useCase.GetRecipeAsync(id);
        
        // Assert
        Assert.Null(result);
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
    }
    
    [Fact]
    public async Task RecipeFound_ReturnsRecipe()
    {
        // Arrange
        var obj = new Recipe();
        const int id = 16;
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(obj);
        
        // Act
        var result = await _useCase.GetRecipeAsync(id);
        
        // Assert
        Assert.NotNull(result);
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
    }
}