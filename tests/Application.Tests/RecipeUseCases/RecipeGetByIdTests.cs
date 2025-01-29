using Application.Recipes;
using Application.Recipes.GetById;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeGetByIdTests
{
    private readonly Mock<IRecipeRepository> _repoMock;
    private readonly RecipeGetById _useCase;

    public RecipeGetByIdTests()
    {
        _repoMock = new Mock<IRecipeRepository>();
        _useCase = new RecipeGetById(_repoMock.Object);
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
    
    [Fact]
    public async Task RecipeFound_SortedComments_ReturnsRecipe()
    {
        // Arrange
        var obj = new Recipe();
        var list = new List<Comment>
        {
            new(new User(), "I am First!", DateTimeOffset.Now - TimeSpan.FromDays(14)),
            new(new User(), "I am Third!", DateTimeOffset.Now - TimeSpan.FromHours(4)),
            new(new User(), "I am Second", DateTimeOffset.Now - TimeSpan.FromDays(7))
        };

        obj.Comments = new List<Comment>(list);
        const int id = 16;
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(obj);

        // Act
        var result = await _useCase.GetRecipeAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Comments);
        Assert.Equal(list.ElementAt(1), result.Comments.ElementAt(0));
        Assert.Equal(list.ElementAt(2), result.Comments.ElementAt(1));
        Assert.Equal(list.ElementAt(0), result.Comments.ElementAt(2));
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
    }
}