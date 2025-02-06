using Application.Recipes;
using Application.Recipes.Delete;
using Application.Users.UseCases;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeDeleteTests
{
    private readonly Mock<IRecipeRepository> _mock;
    private readonly Mock<IUserRepository> _userMock;
    private readonly RecipeDelete _useCase;

    public RecipeDeleteTests()
    {
        _mock = new Mock<IRecipeRepository>();
        _userMock = new Mock<IUserRepository>();
        _useCase = new RecipeDelete(_mock.Object, _userMock.Object);
    }

    [Fact]
    public async Task RecipeNotFound_ReturnsError()
    {
        // Arrange
        const int userId = 15;
        const int recipeId = 16;
        _mock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync((Recipe)null!);

        // Act
        var result = await _useCase.DeleteAsync(recipeId, userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.RecipeNotFound, result.Error);
        _mock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mock.Verify(x => x.DeleteAsync(It.IsAny<RecipeId>()), Times.Never);
    }

    [Fact]
    public async Task UserIsNotAuthor_ReturnsError()
    {
        // Arrange
        const int userId = 15;
        const int recipeId = 16;
        var user = new User { Id = new UserId(userId + 666) };
        var recipe = new Recipe { Author = new User { Id = new UserId(userId) } };
        _mock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);
        _userMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(user);

        // Act
        var result = await _useCase.DeleteAsync(recipeId, userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.UserIsNotAuthor, result.Error);
        _mock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mock.Verify(x => x.DeleteAsync(It.IsAny<RecipeId>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSuccessfully_ReturnsSuccess()
    {
        // Arrange
        const int userId = 15;
        const int recipeId = 16;
        var user = new User { Id = new UserId(userId) };
        var recipe = new Recipe { Author = user };
        _mock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);
        _userMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(user);

        // Act
        var result = await _useCase.DeleteAsync(recipeId, userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mock.Verify(x => x.DeleteAsync(It.IsAny<RecipeId>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSuccessfully_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        const int userId = 15;
        const int recipeId = 16;
        var user = new User { Id = new UserId(userId), Role = UserRole.Admin };
        var recipe = new Recipe { Author = new User { Id = new UserId(userId + 1234) } };
        _mock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(recipe);
        _userMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(user);
        
        // Act
        var result = await _useCase.DeleteAsync(recipeId, userId);
        
        // Assert
        Assert.True(result.IsSuccess);
        _mock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _mock.Verify(x => x.DeleteAsync(It.IsAny<RecipeId>()), Times.Once);
    }
}