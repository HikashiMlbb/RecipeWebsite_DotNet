using Application.Recipes;
using Application.Recipes.GetByPage;
using Domain.RecipeEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeGetByPageTest
{
    private readonly Mock<IRecipeRepository> _mock;
    private readonly RecipeGetByPage _useCase;

    public RecipeGetByPageTest()
    {
        _mock = new Mock<IRecipeRepository>();
        _useCase = new RecipeGetByPage(_mock.Object);
    }

    [Fact]
    public async Task DefaultValues_WhenPageAndPageSizeAreZero()
    {
        // Arrange
        var dto = new RecipeGetByPageDto(0, 0);
        var expectedRecipes = new Recipe[] { new(), new(), new() };
        _mock.Setup(x => x.SearchByPageAsync(1, 10, 0)).ReturnsAsync(expectedRecipes);

        // Act
        var result = await _useCase.GetRecipesAsync(dto);

        // Assert
        Assert.Equal(expectedRecipes, result);
        _mock.Verify(x => x.SearchByPageAsync(1, 10, 0), Times.Once);
    }

    [Fact]
    public async Task DefaultValues_WhenSortTypeIsInvalid()
    {
        // Arrange
        var dto = new RecipeGetByPageDto(0, 0, "invalid");
        var expectedRecipes = new Recipe[] { new(), new(), new() };
        _mock.Setup(x => x.SearchByPageAsync(1, 10, 0)).ReturnsAsync(expectedRecipes);

        // Act
        var result = await _useCase.GetRecipesAsync(dto);

        // Assert
        Assert.Equal(expectedRecipes, result);
        _mock.Verify(x => x.SearchByPageAsync(1, 10, 0), Times.Once);
    }

    [Fact]
    public async Task GetRecipes_ValidParameters_ReturnsRecipesFromRepo()
    {
        // Arrange
        var dto = new RecipeGetByPageDto(2, 20);
        var expectedRecipes = new List<Recipe> { new(), new() }; // Или ожидаемые рецепты
        _mock.Setup(x => x.SearchByPageAsync(2, 20, 0)).ReturnsAsync(expectedRecipes);

        // Act
        var result = await _useCase.GetRecipesAsync(dto);

        // Assert
        Assert.Equal(expectedRecipes, result);
        _mock.Verify(x => x.SearchByPageAsync(2, 20, 0), Times.Once);
    }
}