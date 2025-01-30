using Application.Recipes;
using Application.Recipes.Rate;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeRateTests
{
    private readonly Mock<IRecipeRepository> _repoMock;
    private readonly RecipeRate _useCase;

    public RecipeRateTests()
    {
        _repoMock = new Mock<IRecipeRepository>();
        _useCase = new RecipeRate(_repoMock.Object);
    }

    [Fact]
    public async Task RecipeNotFound_ReturnsError()
    {
        // Arrange
        var dto = new RecipeRateDto(1234, 5678, 1);
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync((Recipe)null!);

        // Act
        var result = await _useCase.Rate(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.RecipeNotFound, result.Error);
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _repoMock.Verify(x => x.RateAsync(It.IsAny<RecipeId>(), It.IsAny<UserId>(), It.IsAny<Stars>()), Times.Never);
    }

    [Fact]
    public async Task StarsAreNotDefined_ReturnsError()
    {
        // Arrange
        var obj = new Recipe();
        var dto1 = new RecipeRateDto(1234, 5678, 0);
        var dto2 = new RecipeRateDto(1234, 5678, -1);
        var dto3 = new RecipeRateDto(1234, 5678, 6);
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(obj);

        // Act
        var result1 = await _useCase.Rate(dto1);
        var result2 = await _useCase.Rate(dto2);
        var result3 = await _useCase.Rate(dto3);

        // Assert
        Assert.False(result1.IsSuccess);
        Assert.Equal(RecipeErrors.StarsAreNotDefined, result1.Error);
        Assert.False(result2.IsSuccess);
        Assert.Equal(RecipeErrors.StarsAreNotDefined, result2.Error);
        Assert.False(result3.IsSuccess);
        Assert.Equal(RecipeErrors.StarsAreNotDefined, result3.Error);
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Exactly(3));
        _repoMock.Verify(x => x.RateAsync(It.IsAny<RecipeId>(), It.IsAny<UserId>(), It.IsAny<Stars>()), Times.Never);
    }
    
    [Fact]
    public async Task UserIsAuthor_ReturnsError()
    {
        // Arrange
        var userId = new UserId(15);
        var obj = new Recipe
        {
            AuthorId = userId
        };
        var dto = new RecipeRateDto(userId.Value, 5678, 5);
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(obj);

        // Act
        var result1 = await _useCase.Rate(dto);

        // Assert
        Assert.False(result1.IsSuccess);
        Assert.Equal(RecipeErrors.UserIsAuthor, result1.Error);
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _repoMock.Verify(x => x.RateAsync(It.IsAny<RecipeId>(), It.IsAny<UserId>(), It.IsAny<Stars>()), Times.Never);
    }

    [Fact]
    public async Task RateSuccessfully_ReturnsSuccess()
    {
        // Arrange
        var obj = new Recipe();
        var dto = new RecipeRateDto(1234, 5678, 1);
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(obj);

        // Act
        var result = await _useCase.Rate(dto);

        // Assert
        Assert.True(result.IsSuccess);
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _repoMock.Verify(x => x.RateAsync(It.IsAny<RecipeId>(), It.IsAny<UserId>(), It.IsAny<Stars>()), Times.Once);
    }
}